using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// Sign in and sign out. Admin and Staff use the same Login; the Role saved in
    /// UserAccounts decides what the account can open.
    /// </summary>
    public class AccountController : Controller
    {
        public const string PasswordResetRateLimitPolicy = "password-reset";

        private const string PasswordResetErrorKey = "PasswordResetError";
        private const string PasswordResetAccountIdKey = "password-reset-account-id";
        private const string PasswordResetStageKey = "password-reset-stage";
        private const string PasswordResetCodeStage = "code";
        private const string PasswordResetVerifiedStage = "verified";
        private const int PasswordResetMaxAttempts = 5;

        private static readonly TimeSpan PasswordResetCodeLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan PasswordResetResendCooldown = TimeSpan.FromMinutes(1);
        private static readonly Regex VerificationCodePattern = new(@"^[0-9]{6}$");
        private static readonly UserAccount PasswordResetTimingAccount = new();
        private static readonly string PasswordResetTimingHash =
            new PasswordHasher<UserAccount>().HashPassword(PasswordResetTimingAccount, Convert.ToHexString(RandomNumberGenerator.GetBytes(16)));

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        private readonly EmailSender _emailSender;

        public AccountController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher, EmailSender emailSender)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
        }

        // -----------------------------------------------------------------------
        // GET /Account/Login
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // A new installation has no accounts yet, so the first Admin is created first.
            if (!_context.UserAccounts.Any())
                return RedirectToAction(nameof(Setup));

            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLocal(returnUrl);

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // -----------------------------------------------------------------------
        // POST /Account/Login
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = model.Username.Trim();
            var account = _context.UserAccounts.FirstOrDefault(u =>
                u.Username == username && (u.Role == UserRoles.Admin || u.Role == UserRoles.Staff));
            var result = account == null
                ? PasswordVerificationResult.Failed
                : _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, model.Password);

            if (account == null || result == PasswordVerificationResult.Failed)
            {
                // The same message for an unknown username and a wrong password.
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account.PasswordHash = _passwordHasher.HashPassword(account, model.Password);
                _context.SaveChanges();
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CreatePrincipal(account));

            // A temporary password must be changed before the account can be used.
            if (account.MustChangePassword)
                return RedirectToAction(nameof(ChangePassword));

            return RedirectToLocal(model.ReturnUrl);
        }

        // -----------------------------------------------------------------------
        // GET /Account/ChangePassword
        // A temporary password must be replaced before the system can be used.
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var account = CurrentAccount();
            if (account == null)
                return RedirectToAction(nameof(Login));
            if (!account.MustChangePassword)
                return RedirectToAction("Index", "Patients");

            return View(new ChangePasswordViewModel());
        }

        // -----------------------------------------------------------------------
        // POST /Account/ChangePassword
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var account = CurrentAccount();
            if (account == null)
                return RedirectToAction(nameof(Login));
            if (!account.MustChangePassword)
                return RedirectToAction("Index", "Patients");

            var passwordError = BasicPasswordError(model.NewPassword);
            if (passwordError != null)
                ModelState.AddModelError(nameof(model.NewPassword), passwordError);
            else if (_passwordHasher.VerifyHashedPassword(account, account.PasswordHash, model.NewPassword!) != PasswordVerificationResult.Failed)
                ModelState.AddModelError(nameof(model.NewPassword), AuthMessages.NewPasswordNotTemporary);

            if (string.IsNullOrEmpty(model.ConfirmPassword))
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.Required);
            else if (model.ConfirmPassword != model.NewPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.PasswordMismatch);

            if (!ModelState.IsValid)
                return View(new ChangePasswordViewModel());

            // A new security stamp signs out any other active sessions.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.NewPassword!);
            account.MustChangePassword = false;
            account.SecurityStamp = NewSecurityStamp();
            _context.SaveChanges();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CreatePrincipal(account));
            return RedirectToAction("Index", "Patients");
        }

        // -----------------------------------------------------------------------
        // POST /Account/Logout
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // -----------------------------------------------------------------------
        // Password recovery: username + registered email -> code -> new password
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword(bool busy = false)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Patients");

            if (busy)
                TempData[PasswordResetErrorKey] = "Please wait a moment before trying again.";

            // Repopulates Username/Email when a code is already pending (e.g. after Resend Code).
            return ForgotPasswordCard();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(PasswordResetRateLimitPolicy)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Username = model.Username.Trim();
            model.Email = NormalizeEmail(model.Email);
            if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Enter a valid email address.");
                return View(model);
            }

            ClearPasswordResetFlow();

            var account = _context.UserAccounts.FirstOrDefault(u =>
                u.Username == model.Username
                && u.RecoveryEmail == model.Email
                && (u.Role == UserRoles.Admin || u.Role == UserRoles.Staff));

            if (account == null)
            {
                // Keep the request cost comparable without revealing whether an account matched.
                _passwordHasher.VerifyHashedPassword(PasswordResetTimingAccount, PasswordResetTimingHash, model.Username + model.Email);
                // Marks both fields red (asp-for picks up ModelState by key) since either could be the mistake.
                ModelState.AddModelError(nameof(model.Username), "Username or email is incorrect.");
                ModelState.AddModelError(nameof(model.Email), "Username or email is incorrect.");
                return View(model);
            }

            if (!account.IsRecoveryEmailVerified)
            {
                ModelState.AddModelError(nameof(model.Email), AuthMessages.NoVerifiedRecoveryEmail);
                return View(model);
            }

            // A code was already sent moments ago: let the user keep using it instead of sending another.
            if (WasPasswordResetCodeSentRecently(account, DateTime.UtcNow))
            {
                StartPasswordResetFlow(account.Id);
                return View(model);
            }

            if (!await GenerateAndSendPasswordResetCodeAsync(account))
            {
                // Marks the Email field, since that is where the send actually failed.
                ModelState.AddModelError(nameof(model.Email), "Unable to send verification code. Please try again.");
                return View(model);
            }

            StartPasswordResetFlow(account.Id);
            return View(model);
        }

        // Verify Code is submitted from the same Forgot Password card (no separate page).
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(PasswordResetRateLimitPolicy)]
        public IActionResult VerifyCode(VerifyPasswordResetCodeViewModel model)
        {
            var code = model.Code?.Trim() ?? string.Empty;
            if (code.Length == 0)
            {
                ModelState.Clear();
                ModelState.AddModelError(nameof(model.Code), "Verification code is required.");
                return ForgotPasswordCard();
            }

            if (!VerificationCodePattern.IsMatch(code))
                return VerificationCodeFailed();

            var account = PasswordResetFlowAccount(PasswordResetCodeStage);
            if (account == null || account.PasswordResetCodeHash == null)
            {
                _passwordHasher.VerifyHashedPassword(PasswordResetTimingAccount, PasswordResetTimingHash, code);
                return VerificationCodeFailed();
            }

            var now = DateTime.UtcNow;
            if (account.PasswordResetCodeExpiresUtc is null || account.PasswordResetCodeExpiresUtc <= now)
            {
                ClearPasswordResetState(account);
                _context.SaveChanges();
                ClearPasswordResetFlow();
                ModelState.AddModelError(nameof(model.Code), "The verification code has expired. Please request a new code.");
                return ForgotPasswordCard(account);
            }

            if (_passwordHasher.VerifyHashedPassword(account, account.PasswordResetCodeHash, code) == PasswordVerificationResult.Failed)
            {
                RegisterVerificationFailure(account);
                return VerificationCodeFailed(account);
            }

            // The code is single-use. A short, server-side verified stage is now required
            // before the password form can be reached or submitted.
            account.PasswordResetCodeHash = null;
            account.PasswordResetCodeExpiresUtc = now.Add(PasswordResetCodeLifetime);
            account.PasswordResetFailedAttempts = 0;
            _context.SaveChanges();
            HttpContext.Session.SetString(PasswordResetStageKey, PasswordResetVerifiedStage);

            return RedirectToAction(nameof(ResetPassword));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(PasswordResetRateLimitPolicy)]
        public async Task<IActionResult> ResendCode()
        {
            var account = PasswordResetFlowAccount(PasswordResetCodeStage);
            if (account == null)
            {
                TempData[PasswordResetErrorKey] = "Please request a new verification code.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var now = DateTime.UtcNow;
            if (WasPasswordResetCodeSentRecently(account, now))
            {
                TempData[PasswordResetErrorKey] = "Please wait a moment before requesting another code.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            if (!await GenerateAndSendPasswordResetCodeAsync(account))
            {
                ClearPasswordResetFlow();
                TempData[PasswordResetErrorKey] = "Unable to send verification code. Please try again.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            return RedirectToAction(nameof(ForgotPassword));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Patients");

            if (VerifiedPasswordResetAccount() == null)
            {
                ClearPasswordResetFlow();
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(PasswordResetRateLimitPolicy)]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var account = VerifiedPasswordResetAccount();
            if (account == null)
            {
                ClearPasswordResetFlow();
                TempData[PasswordResetErrorKey] = "The password reset session has expired. Please request a new verification code.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var passwordError = BasicPasswordError(model.NewPassword);
            if (passwordError != null)
                ModelState.AddModelError(nameof(model.NewPassword), passwordError);
            else if (_passwordHasher.VerifyHashedPassword(account, account.PasswordHash, model.NewPassword!) != PasswordVerificationResult.Failed)
                ModelState.AddModelError(nameof(model.NewPassword), AuthMessages.NewPasswordNotCurrent);

            if (string.IsNullOrEmpty(model.ConfirmPassword))
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.Required);
            else if (model.ConfirmPassword != model.NewPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), AuthMessages.PasswordMismatch);

            if (!ModelState.IsValid)
                return View(new ResetPasswordViewModel());

            account.PasswordHash = _passwordHasher.HashPassword(account, model.NewPassword!);
            account.MustChangePassword = false;
            account.SecurityStamp = NewSecurityStamp();
            ClearPasswordResetState(account);
            _context.SaveChanges();
            ClearPasswordResetFlow();

            TempData["PasswordResetSuccess"] = "Password reset successfully. You can now log in using your new password.";
            return RedirectToAction(nameof(Login));
        }

        // -----------------------------------------------------------------------
        // GET /Account/AccessDenied
        // Shown when a signed-in account opens a page its role cannot use, for
        // example Staff opening User Management.
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction(nameof(Login));

            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // -----------------------------------------------------------------------
        // GET /Account/Setup
        // Only while no accounts exist: creates the first Admin, so the system
        // never ships with a default password.
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Setup()
        {
            // Once an account exists, a second first-admin setup is not allowed.
            if (_context.UserAccounts.Any())
                return SetupClosed();

            return View(new UserFormViewModel());
        }

        // -----------------------------------------------------------------------
        // POST /Account/Setup
        // -----------------------------------------------------------------------
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup([Bind(nameof(UserFormViewModel.FullName), nameof(UserFormViewModel.Username),
            nameof(UserFormViewModel.Email), nameof(UserFormViewModel.Password), nameof(UserFormViewModel.ConfirmPassword))] UserFormViewModel model)
        {
            if (_context.UserAccounts.Any())
                return SetupClosed();

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(UserFormViewModel.Email), "Email address is required.");
            else if (!IsValidEmail(NormalizeEmail(model.Email)))
                ModelState.AddModelError(nameof(UserFormViewModel.Email), "Enter a valid email address.");

            // Apply the same name validation as User Management.
            if (!string.IsNullOrWhiteSpace(model.FullName) && !Patient.IsValidPersonName(model.FullName))
                ModelState.AddModelError(nameof(UserFormViewModel.FullName), AuthMessages.InvalidName);

            if (!ModelState.IsValid)
                return View(model);

            // The first account is always an Admin; the role never comes from the form.
            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                RecoveryEmail = NormalizeEmail(model.Email),
                Role = UserRoles.Admin
            };
            account.PasswordHash = _passwordHasher.HashPassword(account, model.Password!);
            _context.UserAccounts.Add(account);
            _context.SaveChanges();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CreatePrincipal(account));
            return RedirectToAction("Index", "Patients");
        }

        // The signed-in identity: account id, username, full name and role.
        // Also used by Program.cs to refresh the cookie when an account changes.
        [NonAction]
        public static ClaimsPrincipal CreatePrincipal(UserAccount account)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username),
                new(ClaimTypes.GivenName, account.FullName),
                new(ClaimTypes.Role, account.Role),
                // The security stamp changes whenever the password changes.
                new(SecurityStampClaim, account.SecurityStamp ?? string.Empty)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        public const string SecurityStampClaim = "security_stamp";

        // Confirm that the security stamp in the cookie still matches the account.
        [NonAction]
        public static bool StampMatches(ClaimsPrincipal principal, UserAccount account) =>
            (principal.FindFirst(SecurityStampClaim)?.Value ?? string.Empty) == (account.SecurityStamp ?? string.Empty);

        [NonAction]
        public static string NewSecurityStamp() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        // The shared Admin/Staff password policy: 8-100 characters and not all whitespace.
        [NonAction]
        public static string? BasicPasswordError(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return AuthMessages.Required;
            if (password.Length < 8) return AuthMessages.PasswordMin8;
            if (password.Length > 100) return AuthMessages.PasswordMax;
            return null;
        }

        [NonAction]
        public static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

        [NonAction]
        public static bool IsValidEmail(string email) =>
            email.Length is > 0 and <= 256 && new EmailAddressAttribute().IsValid(email);

        // Development sample Staff and Admin accounts. The method is idempotent and
        // only stores password hashes.
        [NonAction]
        public static void EnsureSampleAccounts(IServiceProvider services, ILogger logger)
        {
            var samples = new[]
            {
                (FullName: "Staff One", Username: "Staff1", Password: "Staff123", Role: UserRoles.Staff),
                (FullName: "System Admin", Username: "Admin", Password: "Admin123", Role: UserRoles.Admin)
            };

            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserAccount>>();

            try
            {
                var created = new List<string>();
                foreach (var sample in samples)
                {
                    if (db.UserAccounts.Any(u => u.Username == sample.Username))
                        continue;

                    var account = new UserAccount
                    {
                        FullName = sample.FullName,
                        Username = sample.Username,
                        Role = sample.Role,
                        // Test accounts have no registered email by default.
                        MustChangePassword = false,
                        RecoveryEmail = null,
                        SecurityStamp = NewSecurityStamp()
                    };
                    account.PasswordHash = hasher.HashPassword(account, sample.Password);
                    db.UserAccounts.Add(account);
                    created.Add(sample.Username);
                }

                if (created.Count == 0)
                    return;

                db.SaveChanges();
                logger.LogInformation("Development sample accounts created: {Accounts}.", string.Join(", ", created));
            }
            catch (Exception ex)
            {
                logger.LogError("The development sample accounts could not be created ({ErrorType}). Apply the latest database migration.", ex.GetType().Name);
            }
        }

        private async Task<bool> GenerateAndSendPasswordResetCodeAsync(UserAccount account)
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
            account.PasswordResetCodeExpiresUtc = DateTime.UtcNow.Add(PasswordResetCodeLifetime);
            account.PasswordResetFailedAttempts = 0;
            _context.SaveChanges();

            var sent = await _emailSender.SendPasswordResetCodeAsync(
                account.RecoveryEmail!, account.Username, code, PasswordResetCodeLifetime);
            if (sent)
                return true;

            // Do not leave an active code when mail delivery could not be started.
            ClearPasswordResetState(account);
            _context.SaveChanges();
            return false;
        }

        private static bool WasPasswordResetCodeSentRecently(UserAccount account, DateTime now) =>
            account.PasswordResetCodeHash != null
            && account.PasswordResetCodeExpiresUtc > now.Add(PasswordResetCodeLifetime - PasswordResetResendCooldown);

        private UserAccount? PasswordResetFlowAccount(string expectedStage)
        {
            var accountId = HttpContext.Session.GetInt32(PasswordResetAccountIdKey);
            if (accountId == null || HttpContext.Session.GetString(PasswordResetStageKey) != expectedStage)
                return null;

            return _context.UserAccounts.FirstOrDefault(u => u.Id == accountId
                && (u.Role == UserRoles.Admin || u.Role == UserRoles.Staff));
        }

        private UserAccount? VerifiedPasswordResetAccount()
        {
            var account = PasswordResetFlowAccount(PasswordResetVerifiedStage);
            return account != null
                && account.PasswordResetCodeHash == null
                && account.PasswordResetCodeExpiresUtc > DateTime.UtcNow
                ? account
                : null;
        }

        private void StartPasswordResetFlow(int accountId)
        {
            ClearPasswordResetFlow();
            HttpContext.Session.SetInt32(PasswordResetAccountIdKey, accountId);
            HttpContext.Session.SetString(PasswordResetStageKey, PasswordResetCodeStage);
        }

        private void ClearPasswordResetFlow()
        {
            HttpContext.Session.Remove(PasswordResetAccountIdKey);
            HttpContext.Session.Remove(PasswordResetStageKey);
        }

        private static void ClearPasswordResetState(UserAccount account)
        {
            account.PasswordResetCodeHash = null;
            account.PasswordResetCodeExpiresUtc = null;
            account.PasswordResetFailedAttempts = 0;
        }

        private void RegisterVerificationFailure(UserAccount account)
        {
            account.PasswordResetFailedAttempts++;
            if (account.PasswordResetFailedAttempts >= PasswordResetMaxAttempts)
            {
                ClearPasswordResetState(account);
                ClearPasswordResetFlow();
            }

            _context.SaveChanges();
        }

        private IActionResult VerificationCodeFailed(UserAccount? account = null)
        {
            ModelState.Clear();
            ModelState.AddModelError(nameof(VerifyPasswordResetCodeViewModel.Code), "Invalid verification code.");
            return ForgotPasswordCard(account);
        }

        // Verify Code lives on the Forgot Password card itself, not a separate page. Username/Email
        // are repopulated from the account already tied to the pending reset (session-tracked), never
        // trusted from a posted hidden field, so the user never has to retype them.
        private IActionResult ForgotPasswordCard(UserAccount? account = null)
        {
            account ??= PasswordResetFlowAccount(PasswordResetCodeStage) ?? PasswordResetFlowAccount(PasswordResetVerifiedStage);
            var model = account == null
                ? new ForgotPasswordViewModel()
                : new ForgotPasswordViewModel { Username = account.Username, Email = account.RecoveryEmail ?? string.Empty };

            return View(nameof(ForgotPassword), model);
        }

        // Load the signed-in Admin or Staff account from the database.
        private UserAccount? CurrentAccount() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? _context.UserAccounts.FirstOrDefault(u => u.Id == id && (u.Role == UserRoles.Admin || u.Role == UserRoles.Staff))
                : null;

        // Setup is closed after the first account exists.
        private IActionResult SetupClosed() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Patients") : RedirectToAction(nameof(Login));

        // Only local return URLs are followed, so a link cannot send the user elsewhere.
        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Patients");
    }
}
