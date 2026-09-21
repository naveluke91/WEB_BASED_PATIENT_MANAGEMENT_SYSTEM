using System.Globalization;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// SuperAdmin sign-in (/login.SupAdmin), password recovery by Gmail and Settings.
    /// The SuperAdmin uses the same clinic pages and the same cookie as Admin and Staff
    /// (role claim = SuperAdmin); Users management lives in UserManagementController.
    /// The hidden URL is only for privacy; every SuperAdmin-only action checks the role.
    /// </summary>
    // SuperAdmin ra maka-access (gawas sa login ug recovery).
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class SuperAdminController : Controller
    {
        public const string RateLimitPolicy = "superadmin-sensitive";
        public const string LoginPath = "/login.SupAdmin";

        private const string NoticeKey = "SaNotice";
        private const string ErrorKey = "SaError";
        private const string FormKey = "SaForm";

        private const string GenericLoginError = "Invalid username or password.";
        private const string RecoverySentMessage = "If the recovery information is valid, a recovery code has been sent.";

        // Lockout: 5 ka sayop nga login → 15 minutos.
        private const int MaxFailedLogins = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // Recovery code: 8 ka numero, 10 minutos, 5 ka sulay.
        private const int MaxCodeAttempts = 5;
        private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

        private static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9._-]+$");
        private static readonly Regex CodePattern = new(@"^[0-9]{8}$");

        // Para parehas ang oras sa tubag bisan walay account.
        private static readonly UserAccount TimingAccount = new();
        private static readonly string TimingHash =
            new PasswordHasher<UserAccount>().HashPassword(TimingAccount, Convert.ToHexString(RandomNumberGenerator.GetBytes(16)));

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        private readonly EmailSender _emailSender;

        public SuperAdminController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher, EmailSender emailSender)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
        }

        // Admin/Staff nga naka-login: Access Denied bisan sa login ug recovery pages.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole(UserRoles.SuperAdmin))
                context.Result = Redirect("/Account/AccessDenied");

            base.OnActionExecuting(context);
        }

        // =======================================================================
        // Sign in (sign out = Account/Logout)
        // =======================================================================

        // GET /login.SupAdmin (walay link sa normal nga UI)
        [AllowAnonymous]
        [HttpGet(LoginPath)]
        public IActionResult Login()
        {
            if (User.IsInRole(UserRoles.SuperAdmin))
                return RedirectToAction("Index", "Patients");

            return View(new LoginViewModel());
        }

        // POST /login.SupAdmin
        [AllowAnonymous]
        [HttpPost(LoginPath)]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(new LoginViewModel { Username = model.Username ?? string.Empty });

            var username = model.Username.Trim();
            var now = DateTime.UtcNow;

            // SuperAdmin ra ang pangitaon; Admin ug Staff dili makalusot.
            var account = _context.UserAccounts.FirstOrDefault(u => u.Username == username && u.Role == UserRoles.SuperAdmin);

            if (account == null || account.LockoutEndUtc > now)
            {
                // Parehas nga oras ug mensahe: walay account o naka-lock.
                _passwordHasher.VerifyHashedPassword(TimingAccount, TimingHash, model.Password);
                return LoginFailed(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                // Limitahi ang login attempts.
                account.FailedLoginAttempts++;
                if (account.FailedLoginAttempts >= MaxFailedLogins)
                {
                    account.LockoutEndUtc = now.Add(LockoutDuration);
                    account.FailedLoginAttempts = 0;
                }
                _context.SaveChanges();
                return LoginFailed(model);
            }

            account.FailedLoginAttempts = 0;
            account.LockoutEndUtc = null;
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                account.PasswordHash = _passwordHasher.HashPassword(account, model.Password);
            if (string.IsNullOrEmpty(account.SecurityStamp))
                account.SecurityStamp = AccountController.NewSecurityStamp();
            _context.SaveChanges();

            await SignInSuperAdminAsync(account, now);

            // Temporary password: usbon una. Kung dili, ang parehas nga Patients sa clinic.
            return account.MustChangePassword
                ? RedirectToAction(nameof(ChangePassword))
                : RedirectToAction("Index", "Patients");
        }

        // =======================================================================
        // First sign-in: replace the temporary password
        // =======================================================================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");
            if (!account.MustChangePassword)
                return RedirectToAction("Index", "Patients");

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");
            if (!account.MustChangePassword)
                return RedirectToAction("Index", "Patients");

            AddNewPasswordErrors(account, model.NewPassword, model.ConfirmPassword);
            if (!ModelState.IsValid)
                return View(new ChangePasswordViewModel());

            SetPassword(account, model.NewPassword!);
            _context.SaveChanges();

            // Bag-o nga stamp: i-renew ang cookie.
            await SignInSuperAdminAsync(account, AccountController.SignedInAt(User) ?? DateTime.UtcNow);
            return RedirectToAction("Index", "Patients");
        }

        // =======================================================================
        // Settings (the signed-in SuperAdmin only)
        // =======================================================================

        [HttpGet]
        public IActionResult Settings()
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");

            return View(new SuperAdminSettingsViewModel
            {
                FullName = account.FullName,
                Username = account.Username,
                RecoveryEmail = account.RecoveryEmail
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameViewModel model)
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");

            var errors = new Dictionary<string, string>();
            CheckCurrentPassword(account, model.CurrentPassword, errors);

            var username = model.NewUsername?.Trim() ?? string.Empty;
            if (username.Length == 0)
                errors[nameof(model.NewUsername)] = AuthMessages.Required;
            else if (username.Length < 3 || username.Length > 50)
                errors[nameof(model.NewUsername)] = AuthMessages.UsernameLength;
            else if (!UsernamePattern.IsMatch(username))
                errors[nameof(model.NewUsername)] = AuthMessages.UsernameChars;
            else if (string.Equals(username, account.Username, StringComparison.OrdinalIgnoreCase))
                errors[nameof(model.NewUsername)] = AuthMessages.SameUsername;
            else if (_context.UserAccounts.Any(u => u.Username == username && u.Id != account.Id))
                errors[nameof(model.NewUsername)] = AuthMessages.UsernameInUse;

            if (errors.Count > 0)
                return BackToSettings("username", errors, new { newUsername = model.NewUsername });

            account.Username = username;
            if (!TrySave(errors, nameof(model.NewUsername)))
                return BackToSettings("username", errors, new { newUsername = model.NewUsername });

            // I-renew ang claims (bag-o nga username).
            await SignInSuperAdminAsync(account, AccountController.SignedInAt(User) ?? DateTime.UtcNow);
            TempData[NoticeKey] = "Username changed.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> ChangeOwnPassword(ChangeOwnPasswordViewModel model)
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");

            var errors = new Dictionary<string, string>();
            CheckCurrentPassword(account, model.CurrentPassword, errors);
            AddNewPasswordErrors(account, model.NewPassword, model.ConfirmPassword);
            foreach (var entry in ModelState.Where(e => e.Value?.Errors.Count > 0))
                errors.TryAdd(entry.Key, entry.Value!.Errors[0].ErrorMessage);

            if (errors.Count > 0)
                return BackToSettings("password", errors, new { });

            SetPassword(account, model.NewPassword!);
            _context.SaveChanges();

            // Bag-o nga stamp: ang ubang session ma-logout; kini i-renew.
            await SignInSuperAdminAsync(account, AccountController.SignedInAt(User) ?? DateTime.UtcNow);
            TempData[NoticeKey] = "Password changed.";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public IActionResult ChangeRecoveryEmail(RecoveryEmailViewModel model)
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return RedirectToAction("Login", "Account");

            var errors = new Dictionary<string, string>();
            CheckCurrentPassword(account, model.CurrentPassword, errors);

            var email = NormalizeEmail(model.RecoveryEmail);
            if (email.Length == 0)
                errors[nameof(model.RecoveryEmail)] = AuthMessages.Required;
            else if (!IsValidEmail(email))
                errors[nameof(model.RecoveryEmail)] = AuthMessages.InvalidGmail;

            if (errors.Count > 0)
                return BackToSettings("recovery", errors, new { recoveryEmail = model.RecoveryEmail });

            // SuperAdmin ra ang naay recovery Gmail.
            account.RecoveryEmail = email;
            _context.SaveChanges();

            TempData[NoticeKey] = "Recovery Gmail saved.";
            return RedirectToAction(nameof(Settings));
        }

        // =======================================================================
        // Forgot password → code by Gmail → verify → new password
        // =======================================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var email = NormalizeEmail(model.Email);
            if (!IsValidEmail(email))
            {
                ModelState.AddModelError(nameof(model.Email), email.Length == 0 ? AuthMessages.Required : AuthMessages.InvalidGmail);
                return View(new ForgotPasswordViewModel { Email = model.Email });
            }

            // Secure random: 8 ka numero.
            var code = RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8", CultureInfo.InvariantCulture);
            var account = _context.UserAccounts.FirstOrDefault(u => u.Role == UserRoles.SuperAdmin && u.RecoveryEmail == email);

            if (account != null)
            {
                // Hash ra ang i-save; ang daan nga code mawala.
                account.PasswordResetCodeHash = _passwordHasher.HashPassword(account, code);
                account.PasswordResetCodeExpiresUtc = DateTime.UtcNow.Add(CodeLifetime);
                account.PasswordResetFailedAttempts = 0;
                _context.SaveChanges();

                // I-send sa background aron parehas ang oras sa tubag.
                var recipient = account.RecoveryEmail!;
                _ = Task.Run(() => _emailSender.SendRecoveryCodeAsync(recipient, code, CodeLifetime));
            }
            else
            {
                // Parehas nga trabaho bisan walay account.
                _passwordHasher.HashPassword(TimingAccount, code);
            }

            // Parehas nga mensahe: dili ipakita kung naa ba ang Gmail.
            TempData[NoticeKey] = RecoverySentMessage;
            return RedirectToAction(nameof(VerifyCode));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult VerifyCode() => View(new VerifyCodeViewModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public IActionResult VerifyCode(VerifyCodeViewModel model)
        {
            var code = model.Code?.Trim() ?? string.Empty;
            if (!CodePattern.IsMatch(code))
            {
                ModelState.AddModelError(nameof(model.Code), AuthMessages.CodeFormat);
                return View(new VerifyCodeViewModel());
            }

            // I-check ang recovery code.
            var account = ActiveRecoveryAccount();
            if (account == null)
            {
                _passwordHasher.VerifyHashedPassword(TimingAccount, TimingHash, code);
                return CodeFailed();
            }

            if (_passwordHasher.VerifyHashedPassword(account, account.PasswordResetCodeHash!, code) == PasswordVerificationResult.Failed)
            {
                RegisterCodeFailure(account);
                return CodeFailed();
            }

            // Sakto: ilisan ang code og usa ka reset token (single use).
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            account.PasswordResetCodeHash = _passwordHasher.HashPassword(account, token);
            account.PasswordResetCodeExpiresUtc = DateTime.UtcNow.Add(CodeLifetime);
            account.PasswordResetFailedAttempts = 0;
            _context.SaveChanges();

            return View(nameof(ResetPassword), new RecoveryResetViewModel { Token = token });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword() => RedirectToAction(nameof(ForgotPassword));

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public IActionResult ResetPassword(RecoveryResetViewModel model)
        {
            var token = model.Token ?? string.Empty;
            var account = ActiveRecoveryAccount();

            // Ang token lang (dili ang 8-digit code) ang dawaton dinhi.
            var valid = account != null && token.Length >= 32
                && _passwordHasher.VerifyHashedPassword(account, account.PasswordResetCodeHash!, token) != PasswordVerificationResult.Failed;

            if (!valid)
            {
                if (account != null)
                    RegisterCodeFailure(account);
                TempData[ErrorKey] = "The recovery session expired. Request a new code.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            AddNewPasswordErrors(account!, model.NewPassword, model.ConfirmPassword);
            if (!ModelState.IsValid)
                return View(new RecoveryResetViewModel { Token = token });

            // Human sa reset: ang code/token dili na magamit.
            SetPassword(account!, model.NewPassword!);
            _context.SaveChanges();

            TempData[NoticeKey] = "Password changed. Sign in with your new password.";
            return Redirect(LoginPath);
        }

        // =======================================================================
        // Used by Program.cs
        // =======================================================================

        // Usa ra ka SuperAdmin: himuon sa startup kung wala pa.
        [NonAction]
        public static void EnsureSuperAdmin(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserAccount>>();

            try
            {
                if (db.UserAccounts.Any(u => u.Role == UserRoles.SuperAdmin))
                    return;

                // Gikan sa user-secrets o environment variables, dili sa code.
                var section = configuration.GetSection("SuperAdminBootstrap");
                var username = section["Username"]?.Trim();
                var password = section["Password"];
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("No SuperAdmin account exists. Set SuperAdminBootstrap:Username and SuperAdminBootstrap:Password (dotnet user-secrets or environment variables) to create it.");
                    return;
                }

                if (db.UserAccounts.Any(u => u.Username == username))
                {
                    logger.LogWarning("The SuperAdmin account was not created because its bootstrap username is already used.");
                    return;
                }

                var recoveryEmail = NormalizeEmail(section["RecoveryEmail"]);
                var fullName = section["FullName"]?.Trim();
                var account = new UserAccount
                {
                    FullName = string.IsNullOrEmpty(fullName) ? "Super Administrator" : fullName,
                    Username = username,
                    Role = UserRoles.SuperAdmin,
                    RecoveryEmail = IsValidEmail(recoveryEmail) ? recoveryEmail : null,
                    // Temporary password: usbon sa una nga login.
                    MustChangePassword = true,
                    SecurityStamp = AccountController.NewSecurityStamp()
                };
                account.PasswordHash = hasher.HashPassword(account, password);
                db.UserAccounts.Add(account);
                db.SaveChanges();

                logger.LogInformation("The SuperAdmin account was created. Its temporary password must be changed at first sign-in.");
            }
            catch (Exception ex)
            {
                logger.LogError("The SuperAdmin account could not be checked or created ({ErrorType}). Apply the latest database migration.", ex.GetType().Name);
            }
        }

        // Rate limit: balik sa kilala nga GET page ra (walay open redirect).
        [NonAction]
        public static string BusyPage(PathString path)
        {
            var value = path.Value ?? string.Empty;
            bool Is(string page) => value.Equals(page, StringComparison.OrdinalIgnoreCase);

            var target = Is("/SuperAdmin/ForgotPassword") || Is("/SuperAdmin/ResetPassword") ? "/SuperAdmin/ForgotPassword"
                : Is("/SuperAdmin/VerifyCode") ? "/SuperAdmin/VerifyCode"
                : Is("/SuperAdmin/ChangePassword") ? "/SuperAdmin/ChangePassword"
                : Is("/SuperAdmin/ChangeUsername") || Is("/SuperAdmin/ChangeOwnPassword") || Is("/SuperAdmin/ChangeRecoveryEmail") ? "/SuperAdmin/Settings"
                : LoginPath;
            return target + "?busy=1";
        }

        // Lig-on nga password: 12+ ka karakter, dako ug gamay nga letra, numero, simbolo.
        [NonAction]
        public static string? StrongPasswordError(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return AuthMessages.Required;
            if (password.Length < 12) return AuthMessages.PasswordMin12;
            if (password.Length > 100) return AuthMessages.PasswordMax;
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit)
                || !password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                return AuthMessages.PasswordComplexity;
            return null;
        }

        // =======================================================================
        // Helpers
        // =======================================================================

        // Parehas nga cookie sa clinic; ang SuperAdmin naay mubo nga idle timeout.
        private Task SignInSuperAdminAsync(UserAccount account, DateTime signedInUtc) =>
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                AccountController.CreatePrincipal(account, signedInUtc), AccountController.SuperAdminProperties());

        private UserAccount? CurrentSuperAdmin() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? _context.UserAccounts.FirstOrDefault(u => u.Id == id && u.Role == UserRoles.SuperAdmin)
                : null;

        // Ang SuperAdmin kay walay code nga aktibo kung expired na.
        private UserAccount? ActiveRecoveryAccount()
        {
            var now = DateTime.UtcNow;
            return _context.UserAccounts.FirstOrDefault(u => u.Role == UserRoles.SuperAdmin
                && u.PasswordResetCodeHash != null && u.PasswordResetCodeExpiresUtc > now);
        }

        // 5 ka sayop: ang code dili na magamit.
        private void RegisterCodeFailure(UserAccount account)
        {
            account.PasswordResetFailedAttempts++;
            if (account.PasswordResetFailedAttempts >= MaxCodeAttempts)
            {
                account.PasswordResetCodeHash = null;
                account.PasswordResetCodeExpiresUtc = null;
                account.PasswordResetFailedAttempts = 0;
            }
            _context.SaveChanges();
        }

        private IActionResult CodeFailed()
        {
            ModelState.AddModelError(nameof(VerifyCodeViewModel.Code), AuthMessages.CodeInvalid);
            return View(nameof(VerifyCode), new VerifyCodeViewModel());
        }

        private IActionResult LoginFailed(LoginViewModel model)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, GenericLoginError);
            return View(nameof(Login), new LoginViewModel { Username = model.Username ?? string.Empty });
        }

        // I-hash ang bag-o nga password ug limpyohan ang recovery ug lockout.
        private void SetPassword(UserAccount account, string newPassword)
        {
            account.PasswordHash = _passwordHasher.HashPassword(account, newPassword);
            account.MustChangePassword = false;
            account.SecurityStamp = AccountController.NewSecurityStamp();
            account.PasswordResetCodeHash = null;
            account.PasswordResetCodeExpiresUtc = null;
            account.PasswordResetFailedAttempts = 0;
            account.FailedLoginAttempts = 0;
            account.LockoutEndUtc = null;
        }

        private void AddNewPasswordErrors(UserAccount account, string? newPassword, string? confirmPassword)
        {
            var error = StrongPasswordError(newPassword);
            if (error != null)
                ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), error);
            else if (_passwordHasher.VerifyHashedPassword(account, account.PasswordHash, newPassword!) != PasswordVerificationResult.Failed)
                ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), AuthMessages.NewPasswordNotCurrent);

            if (string.IsNullOrEmpty(confirmPassword))
                ModelState.AddModelError(nameof(ChangePasswordViewModel.ConfirmPassword), AuthMessages.Required);
            else if (confirmPassword != newPassword)
                ModelState.AddModelError(nameof(ChangePasswordViewModel.ConfirmPassword), AuthMessages.PasswordMismatch);
        }

        private void CheckCurrentPassword(UserAccount account, string? currentPassword, Dictionary<string, string> errors)
        {
            if (string.IsNullOrEmpty(currentPassword))
                errors["CurrentPassword"] = AuthMessages.Required;
            else if (_passwordHasher.VerifyHashedPassword(account, account.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
                errors["CurrentPassword"] = AuthMessages.CurrentPasswordIncorrect;
        }

        private bool TrySave(Dictionary<string, string> errors, string usernameField)
        {
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                errors[usernameField] = AuthMessages.UsernameInUse;
                return false;
            }
        }

        // Balik sa Settings; markahan ang mga field (walay password nga ibalik).
        private IActionResult BackToSettings(string form, Dictionary<string, string> errors, object values)
        {
            TempData[FormKey] = JsonSerializer.Serialize(new { form, errors, values });
            return RedirectToAction(nameof(Settings));
        }

        private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

        private static bool IsValidEmail(string email) =>
            email.Length is > 0 and <= 256
            && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            && MailAddress.TryCreate(email, out var address) && address.Address == email;
    }
}
