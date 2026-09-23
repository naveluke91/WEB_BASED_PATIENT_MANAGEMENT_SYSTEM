using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// User Management. Lists the accounts that can sign in and adds, edits and
    /// deletes them. Healthcare records are not linked to user accounts, so
    /// deleting an account never removes patient, consultation or billing data.
    /// Admin manages both Admin and Staff accounts, but an Admin account can
    /// never be deleted here.
    /// </summary>
    // Admin can manage Staff accounts; Staff is denied access.
    [Authorize(Roles = UserRoles.Admin)]
    public class UserManagementController : Controller
    {
        private const string NotAllowedMessage = "You can't manage that account.";
        private const string AdminNotDeletableMessage = "Admin accounts cannot be deleted.";

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;

        public UserManagementController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
        // Admin picks the role (Admin or Staff); anything else is rejected.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(nameof(UserFormViewModel.FullName), nameof(UserFormViewModel.Username),
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
                SecurityStamp = AccountController.NewSecurityStamp()
            };
            // Only the salted hash is stored.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.Password!);
            _context.UserAccounts.Add(account);

            if (!TrySave())
                return ReopenForm("add", model);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was added.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Edit
        // Updates the name, username, and registered email for an Admin or Staff
        // account. The role and the password are not changed here.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind(nameof(UserFormViewModel.Id), nameof(UserFormViewModel.FullName),
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
            if (!string.IsNullOrEmpty(model.Email)
                && !string.Equals(account.RecoveryEmail, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                account.RecoveryEmail = model.Email;
                ClearPasswordResetState(account);
            }

            if (!TrySave())
                return ReopenForm("edit", model);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/UpdateOwnEmail
        // The target is always the signed-in Admin; no account id is accepted
        // from the browser.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOwnEmail(UpdateOwnEmailViewModel model)
        {
            var account = CurrentAdmin();
            if (account == null)
                return Forbid();

            var passwordResult = PasswordVerificationResult.Failed;
            if (string.IsNullOrEmpty(model.CurrentPassword))
                ModelState.AddModelError(nameof(model.CurrentPassword), AuthMessages.CurrentPasswordRequired);
            else
            {
                passwordResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, model.CurrentPassword);
                if (passwordResult == PasswordVerificationResult.Failed)
                    ModelState.AddModelError(nameof(model.CurrentPassword), AuthMessages.CurrentPasswordIncorrect);
            }

            var email = NormalizeEmail(model.Email);
            if (email.Length == 0)
                ModelState.AddModelError(nameof(model.Email), AuthMessages.EmailRequired);
            else if (!IsValidEmail(email))
                ModelState.AddModelError(nameof(model.Email), AuthMessages.InvalidEmail);

            if (!ModelState.IsValid)
                return ReopenEmailForm(model);

            if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
                account.PasswordHash = _passwordHasher.HashPassword(account, model.CurrentPassword!);

            account.RecoveryEmail = email;
            ClearPasswordResetState(account);
            _context.SaveChanges();
            // "SuccessMessage" is cleared by _ViewStart before clinic pages render (see ReopenForm's siblings).
            TempData["UserNotice"] = "Your email address was updated.";
            return RedirectToAction(nameof(Index));
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

        // Same reopen pattern as ReopenForm, for the Update My Email modal.
        private IActionResult ReopenEmailForm(UpdateOwnEmailViewModel model)
        {
            var error = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                ?? "Please check the email details.";

            TempData["UserForm"] = JsonSerializer.Serialize(new
            {
                mode = "email",
                email = model.Email,
                error,
                field = ModelState.FirstOrDefault(entry => entry.Value?.Errors.Count > 0).Key
            });

            return RedirectToAction(nameof(Index));
        }

        private UserAccount? CurrentAdmin() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? _context.UserAccounts.FirstOrDefault(u => u.Id == id && u.Role == UserRoles.Admin)
                : null;

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
