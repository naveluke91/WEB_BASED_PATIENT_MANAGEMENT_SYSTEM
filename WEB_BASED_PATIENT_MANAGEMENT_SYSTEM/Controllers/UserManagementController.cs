using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// User Management. Lists the accounts that can sign in and adds, edits and
    /// deletes them. Healthcare records are not linked to user accounts, so
    /// deleting an account never removes patient, consultation or billing data.
    /// Admin manages both Admin and Staff accounts, but an Admin account can
    /// never be deleted here. A new or changed Recovery Email must be proven
    /// reachable with a MailKit-sent code before Forgot Password will trust it.
    /// </summary>
    // Admin can manage Staff accounts; Staff is denied access.
    [Authorize(Roles = UserRoles.Admin)]
    public class UserManagementController : Controller
    {
        private const string NotAllowedMessage = "You can't manage that account.";
        private const string AdminNotDeletableMessage = "Admin accounts cannot be deleted.";

        private const int EmailVerificationMaxAttempts = 5;
        private static readonly TimeSpan EmailVerificationCodeLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan EmailVerificationResendCooldown = TimeSpan.FromMinutes(1);
        private static readonly Regex VerificationCodePattern = new(@"^[0-9]{6}$");

        // Session-tracked, this Admin's own pending verification only (not the target account's).
        private const string PendingCreateVerifyAccountIdKey = "usermgmt-pending-create-verify-id";
        private const string PendingEditVerifyAccountIdKey = "usermgmt-pending-edit-verify-id";
        private const string PendingEditNewEmailKey = "usermgmt-pending-edit-new-email";

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        private readonly EmailSender _emailSender;

        public UserManagementController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher, EmailSender emailSender)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
        }

        // Edit can target either an Admin or a Staff account (role itself is never editable).
        private UserAccount? ManagedAccount(int id)
        {
            return _context.UserAccounts.FirstOrDefault(u => u.Id == id
                && (u.Role == UserRoles.Admin || u.Role == UserRoles.Staff));
        }

        // -----------------------------------------------------------------------
        // GET /UserManagement
        // -----------------------------------------------------------------------
        public IActionResult Index()
        {
            var users = _context.UserAccounts
                .AsNoTracking()
                .Where(u => u.Role == UserRoles.Admin || u.Role == UserRoles.Staff)
                .OrderBy(u => u.FullName)
                .ToList();

            return View(new UserManagementPageViewModel { Users = users });
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Create
        // Admin picks the role (Admin or Staff); anything else is rejected. The
        // account is created right away but stays unverified until the entered
        // Recovery Email is proven reachable by a MailKit-sent code.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(nameof(UserFormViewModel.FullName), nameof(UserFormViewModel.Username),
            nameof(UserFormViewModel.Email), nameof(UserFormViewModel.Role), nameof(UserFormViewModel.Password),
            nameof(UserFormViewModel.ConfirmPassword))] UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            ValidateUsernameIsFree(model.Username, exceptId: null);
            ValidateFullName(model.FullName);
            model.Email = NormalizeEmail(model.Email);
            ValidateEmail(model.Email, required: true, nameof(UserFormViewModel.Email));

            if (model.Role != UserRoles.Admin && model.Role != UserRoles.Staff)
                ModelState.AddModelError(nameof(UserFormViewModel.Role), "Select a valid role.");

            if (!ModelState.IsValid)
                return ReopenForm("add", model);

            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Role = model.Role!,
                RecoveryEmail = model.Email,
                IsRecoveryEmailVerified = false,
                SecurityStamp = AccountController.NewSecurityStamp()
            };
            // Only the salted hash is stored.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.Password!);
            _context.UserAccounts.Add(account);

            if (!TrySave())
                return ReopenForm("add", model);

            HttpContext.Session.SetInt32(PendingCreateVerifyAccountIdKey, account.Id);
            var sent = await GenerateAndSendEmailVerificationCodeAsync(account, account.RecoveryEmail!);
            return ReopenVerifyForm("addVerify", account, account.RecoveryEmail!,
                sent ? null : "Unable to send verification code. Please try again.");
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/VerifyNewAccountEmail
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyNewAccountEmail(string? code)
        {
            var accountId = HttpContext.Session.GetInt32(PendingCreateVerifyAccountIdKey);
            var account = accountId.HasValue ? _context.UserAccounts.FirstOrDefault(u => u.Id == accountId) : null;
            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            var (success, error) = VerifyPendingCode(account, code);
            if (!success)
                return ReopenVerifyForm("addVerify", account, account.RecoveryEmail!, error);

            account.IsRecoveryEmailVerified = true;
            _context.SaveChanges();
            HttpContext.Session.Remove(PendingCreateVerifyAccountIdKey);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was added.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/ResendNewAccountVerificationCode
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendNewAccountVerificationCode()
        {
            var accountId = HttpContext.Session.GetInt32(PendingCreateVerifyAccountIdKey);
            var account = accountId.HasValue ? _context.UserAccounts.FirstOrDefault(u => u.Id == accountId) : null;
            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            if (WasCodeSentRecently(account, DateTime.UtcNow))
                return ReopenVerifyForm("addVerify", account, account.RecoveryEmail!, "Please wait a moment before requesting another code.");

            var sent = await GenerateAndSendEmailVerificationCodeAsync(account, account.RecoveryEmail!);
            return ReopenVerifyForm("addVerify", account, account.RecoveryEmail!,
                sent ? null : "Unable to send verification code. Please try again.");
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Edit
        // Updates the name and username immediately. A changed Recovery Email is
        // NOT applied yet: the old (already verified) email stays active until
        // the new one is proven reachable by a MailKit-sent code.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind(nameof(UserFormViewModel.Id), nameof(UserFormViewModel.FullName),
            nameof(UserFormViewModel.Username), nameof(UserFormViewModel.Email))] UserFormViewModel model)
        {
            var account = ManagedAccount(model.Id);
            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            ValidateUsernameIsFree(model.Username, exceptId: account.Id);
            ValidateFullName(model.FullName);
            model.Email = NormalizeEmail(model.Email);
            ValidateEmail(model.Email, required: false, nameof(UserFormViewModel.Email));

            if (!ModelState.IsValid)
                return ReopenForm("edit", model);

            account.FullName = model.FullName.Trim();
            account.Username = model.Username.Trim();

            var emailChanged = !string.IsNullOrEmpty(model.Email)
                && !string.Equals(account.RecoveryEmail, model.Email, StringComparison.OrdinalIgnoreCase);

            if (!TrySave())
                return ReopenForm("edit", model);

            if (!emailChanged)
            {
                TempData["SuccessMessage"] = $"User \"{account.FullName}\" was updated.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetInt32(PendingEditVerifyAccountIdKey, account.Id);
            HttpContext.Session.SetString(PendingEditNewEmailKey, model.Email!);
            var sent = await GenerateAndSendEmailVerificationCodeAsync(account, model.Email!);
            return ReopenVerifyForm("editVerify", account, model.Email!,
                sent ? null : "Unable to send verification code. Please try again.");
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/VerifyEditedAccountEmail
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyEditedAccountEmail(string? code)
        {
            var accountId = HttpContext.Session.GetInt32(PendingEditVerifyAccountIdKey);
            var pendingEmail = HttpContext.Session.GetString(PendingEditNewEmailKey);
            var account = accountId.HasValue ? ManagedAccount(accountId.Value) : null;
            if (account == null || string.IsNullOrEmpty(pendingEmail))
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            var (success, error) = VerifyPendingCode(account, code);
            if (!success)
                return ReopenVerifyForm("editVerify", account, pendingEmail, error);

            // Only now does the new email replace the old (previously verified) one.
            account.RecoveryEmail = pendingEmail;
            account.IsRecoveryEmailVerified = true;
            _context.SaveChanges();
            HttpContext.Session.Remove(PendingEditVerifyAccountIdKey);
            HttpContext.Session.Remove(PendingEditNewEmailKey);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/ResendEditedAccountVerificationCode
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEditedAccountVerificationCode()
        {
            var accountId = HttpContext.Session.GetInt32(PendingEditVerifyAccountIdKey);
            var pendingEmail = HttpContext.Session.GetString(PendingEditNewEmailKey);
            var account = accountId.HasValue ? ManagedAccount(accountId.Value) : null;
            if (account == null || string.IsNullOrEmpty(pendingEmail))
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            if (WasCodeSentRecently(account, DateTime.UtcNow))
                return ReopenVerifyForm("editVerify", account, pendingEmail, "Please wait a moment before requesting another code.");

            var sent = await GenerateAndSendEmailVerificationCodeAsync(account, pendingEmail);
            return ReopenVerifyForm("editVerify", account, pendingEmail,
                sent ? null : "Unable to send verification code. Please try again.");
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/ResetPassword
        // Sets a new password for a Staff account. The Staff member can sign in
        // with it immediately. Staff cannot reach this action.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetAccountPasswordViewModel model)
        {
            // Kuhaa ang target sa database ug i-check ang tinuod nga Role niini;
            // ang role gikan sa browser dili gamiton.
            var account = _context.UserAccounts.FirstOrDefault(u => u.Id == model.Id);
            if (account == null)
            {
                TempData["ErrorMessage"] = AuthMessages.AccountNotFound;
                return RedirectToAction(nameof(Index));
            }

            if (account.Role != UserRoles.Staff)
            {
                TempData["ErrorMessage"] = AuthMessages.ResetStaffOnly;
                return RedirectToAction(nameof(Index));
            }

            var passwordError = AccountController.BasicPasswordError(model.NewPassword);
            if (passwordError != null)
                ModelState.AddModelError(nameof(model.NewPassword), passwordError);
            if (string.IsNullOrEmpty(model.ConfirmPassword))
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.Required);
            else if (model.ConfirmPassword != model.NewPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.PasswordMismatch);

            if (!ModelState.IsValid)
                return ReopenForm("reset", new UserFormViewModel { Id = account.Id, FullName = account.FullName });

            // I-hash ang bag-ong password; mawala ang daan nga session.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.NewPassword!);

            // The Staff member can sign in with the new password immediately.
            account.MustChangePassword = false;
            account.SecurityStamp = AccountController.NewSecurityStamp();
            ClearPasswordResetState(account);
            _context.SaveChanges();

            // Ang password dili ipakita o i-log.
            TempData["UserNotice"] = $"Password for \"{account.FullName}\" was reset successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Delete/5
        // Staff accounts can be deleted; Admin accounts cannot, enforced here
        // regardless of what the browser sends.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var account = ManagedAccount(id);

            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
            }
            else if (account.Role == UserRoles.Admin)
            {
                TempData["ErrorMessage"] = AdminNotDeletableMessage;
            }
            else
            {
                // Login account ra; ang patient records dili apil.
                _context.UserAccounts.Remove(account);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"User \"{account.FullName}\" was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Valid nga ngalan lang (parehas sa Patient).
        private void ValidateFullName(string? fullName)
        {
            if (!string.IsNullOrWhiteSpace(fullName) && !Patient.IsValidPersonName(fullName))
                ModelState.AddModelError(nameof(UserFormViewModel.FullName), AuthMessages.InvalidName);
        }

        private void ValidateUsernameIsFree(string? requestedUsername, int? exceptId)
        {
            var username = requestedUsername?.Trim();
            if (string.IsNullOrEmpty(username))
                return;

            if (_context.UserAccounts.Any(u => u.Username == username && u.Id != exceptId))
                ModelState.AddModelError(nameof(UserFormViewModel.Username), AuthMessages.UsernameInUse);
        }

        private void ValidateEmail(string? requestedEmail, bool required, string fieldName)
        {
            var email = NormalizeEmail(requestedEmail);
            if (email.Length == 0)
            {
                if (required)
                    ModelState.AddModelError(fieldName, AuthMessages.EmailRequired);
                return;
            }

            if (!IsValidEmail(email))
                ModelState.AddModelError(fieldName, AuthMessages.InvalidEmail);
        }

        // The unique Username index can still reject a name saved at the same moment.
        private bool TrySave()
        {
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(UserFormViewModel.Username), AuthMessages.UsernameInUse);
                return false;
            }
        }

        // Back to the list with the same modal reopened and the first error shown.
        // Passwords are never sent back to the page.
        private IActionResult ReopenForm(string mode, UserFormViewModel model)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                ?? "Please check the user details.";

            TempData["UserForm"] = JsonSerializer.Serialize(new
            {
                mode,
                id = model.Id,
                fullName = model.FullName,
                username = model.Username,
                email = model.Email,
                role = model.Role,
                error,
                // Ang field nga may sayop (para ma-marka og pula).
                field = ModelState.FirstOrDefault(entry => entry.Value?.Errors.Count > 0).Key
            });

            return RedirectToAction(nameof(Index));
        }

        // Reopens the Add/Edit modal at its "enter the code" stage. error == null means the code
        // was just sent with nothing wrong yet; the Verification Code field carries no message then.
        private IActionResult ReopenVerifyForm(string mode, UserAccount account, string email, string? error)
        {
            TempData["UserForm"] = JsonSerializer.Serialize(new
            {
                mode,
                id = account.Id,
                fullName = account.FullName,
                username = account.Username,
                email,
                role = account.Role,
                error,
                field = error != null ? "Code" : null
            });

            return RedirectToAction(nameof(Index));
        }

        // Shared by account-creation and edited-email verification. Reuses the same
        // code-hash/expiry/attempts fields the Forgot Password flow uses, since this
        // account already exists either way by the time a code is being checked.
        private (bool Success, string? Error) VerifyPendingCode(UserAccount account, string? code)
        {
            var trimmed = code?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                return (false, "Verification code is required.");

            if (!VerificationCodePattern.IsMatch(trimmed) || account.PasswordResetCodeHash == null)
                return (false, "Invalid verification code.");

            var now = DateTime.UtcNow;
            if (account.PasswordResetCodeExpiresUtc is null || account.PasswordResetCodeExpiresUtc <= now)
            {
                ClearPasswordResetState(account);
                _context.SaveChanges();
                return (false, "The verification code has expired. Please request a new code.");
            }

            if (_passwordHasher.VerifyHashedPassword(account, account.PasswordResetCodeHash, trimmed) == PasswordVerificationResult.Failed)
            {
                account.PasswordResetFailedAttempts++;
                if (account.PasswordResetFailedAttempts >= EmailVerificationMaxAttempts)
                    ClearPasswordResetState(account);
                _context.SaveChanges();
                return (false, "Invalid verification code.");
            }

            // Single-use: cleared whether this call is consumed as a success or not reused again.
            ClearPasswordResetState(account);
            return (true, null);
        }

        private async Task<bool> GenerateAndSendEmailVerificationCodeAsync(UserAccount account, string email)
        {
            var previousCodeHash = account.PasswordResetCodeHash;
            string code;
            do
            {
                code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
            }
            while (previousCodeHash != null
                && _passwordHasher.VerifyHashedPassword(account, previousCodeHash, code) != PasswordVerificationResult.Failed);

            account.PasswordResetCodeHash = _passwordHasher.HashPassword(account, code);
            account.PasswordResetCodeExpiresUtc = DateTime.UtcNow.Add(EmailVerificationCodeLifetime);
            account.PasswordResetFailedAttempts = 0;
            _context.SaveChanges();

            var sent = await _emailSender.SendEmailVerificationCodeAsync(email, account.Username, code, EmailVerificationCodeLifetime);
            if (sent)
                return true;

            // Do not leave an active code when mail delivery could not be started.
            ClearPasswordResetState(account);
            _context.SaveChanges();
            return false;
        }

        private static bool WasCodeSentRecently(UserAccount account, DateTime now) =>
            account.PasswordResetCodeHash != null
            && account.PasswordResetCodeExpiresUtc > now.Add(EmailVerificationCodeLifetime - EmailVerificationResendCooldown);

        private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

        private static bool IsValidEmail(string email) =>
            email.Length is > 0 and <= 256
            && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            && MailAddress.TryCreate(email, out var address)
            && address.Address == email;

        private static void ClearPasswordResetState(UserAccount account)
        {
            account.PasswordResetCodeHash = null;
            account.PasswordResetCodeExpiresUtc = null;
            account.PasswordResetFailedAttempts = 0;
        }
    }
}
