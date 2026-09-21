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
    /// SuperAdmin console with its own sign-in (/login.SupAdmin), cookie and theme.
    /// Manages the Admin and Staff accounts, its own settings and Gmail recovery.
    /// The hidden URL is only for privacy; every action checks the SuperAdmin role.
    /// </summary>
    // SuperAdmin ra maka-access (lahi nga cookie).
    [Authorize(AuthenticationSchemes = Scheme, Roles = UserRoles.SuperAdmin)]
    public class SuperAdminController : Controller
    {
        public const string Scheme = "SuperAdmin";
        public const string RateLimitPolicy = "superadmin-sensitive";
        public const string LoginPath = "/login.SupAdmin";

        // Session: 30 minutos nga walay lihok (Program.cs), 4 ka oras labing dugay.
        public static readonly TimeSpan MaxSessionAge = TimeSpan.FromHours(4);

        private const string SignedInClaim = "superadmin_signed_in";
        private const string NoticeKey = "SaNotice";
        private const string ErrorKey = "SaError";
        private const string FormKey = "SaForm";

        private const string GenericLoginError = "Invalid username or password.";
        private const string RecoverySentMessage = "If the recovery information is valid, a recovery code has been sent.";
        private const string InvalidCodeMessage = "Invalid or expired code.";

        // Lockout: 5 ka sayop nga login → 15 minutos.
        private const int MaxFailedLogins = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // Recovery code: 8 ka numero, 10 minutos, 5 ka sulay.
        private const int MaxCodeAttempts = 5;
        private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

        private static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9._-]{3,50}$");
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

        // Admin/Staff nga naka-login sa clinic: Access Denied bisan sa login ug recovery pages.
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!User.IsInRole(UserRoles.SuperAdmin)
                && (await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)).Succeeded)
            {
                context.Result = Redirect("/Account/AccessDenied");
                return;
            }

            await next();
        }

        // =======================================================================
        // Sign in / sign out
        // =======================================================================

        // GET /login.SupAdmin (walay link sa normal nga UI)
        [AllowAnonymous]
        [HttpGet(LoginPath)]
        public IActionResult Login()
        {
            if (User.IsInRole(UserRoles.SuperAdmin))
                return RedirectToAction(nameof(ManageUsers));

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

            await HttpContext.SignInAsync(Scheme, CreateSuperAdminPrincipal(account, now));

            // Temporary password: usbon una.
            return account.MustChangePassword
                ? RedirectToAction(nameof(ChangePassword))
                : RedirectToAction(nameof(ManageUsers));
        }

        // POST /SuperAdmin/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Scheme);
            return Redirect(LoginPath);
        }

        // =======================================================================
        // First sign-in: replace the temporary password
        // =======================================================================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return Redirect(LoginPath);
            if (!account.MustChangePassword)
                return RedirectToAction(nameof(ManageUsers));

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicy)]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return Redirect(LoginPath);
            if (!account.MustChangePassword)
                return RedirectToAction(nameof(ManageUsers));

            AddNewPasswordErrors(account, model.NewPassword, model.ConfirmPassword);
            if (!ModelState.IsValid)
                return View(new ChangePasswordViewModel());

            SetPassword(account, model.NewPassword!);
            _context.SaveChanges();

            // Bag-o nga stamp: i-renew ang cookie.
            await HttpContext.SignInAsync(Scheme, CreateSuperAdminPrincipal(account, SignedInAt(User) ?? DateTime.UtcNow));
            TempData[NoticeKey] = "Password changed.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // =======================================================================
        // Manage Users (Admin and Staff only)
        // =======================================================================

        [HttpGet]
        public IActionResult ManageUsers()
        {
            // Admin ug Staff ra; ang SuperAdmin dili ma-lista.
            var accounts = _context.UserAccounts
                .AsNoTracking()
                .Where(u => u.Role != UserRoles.SuperAdmin)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToList();

            return View(new SuperAdminUsersPageViewModel { Accounts = accounts });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAccount([Bind(nameof(SuperAdminAccountFormViewModel.FullName), nameof(SuperAdminAccountFormViewModel.Username),
            nameof(SuperAdminAccountFormViewModel.Password), nameof(SuperAdminAccountFormViewModel.ConfirmPassword),
            nameof(SuperAdminAccountFormViewModel.Role))] SuperAdminAccountFormViewModel model)
        {
            var errors = ModelErrors();
            // Admin o Staff ra; ang SuperAdmin i-reject.
            var role = AssignableRole(model.Role, errors);
            ValidateAccountFields(model.FullName, model.Username, exceptId: null, errors);
            var passwordError = AccountController.BasicPasswordError(model.Password);
            if (passwordError != null)
                errors[nameof(model.Password)] = passwordError;

            var values = new { fullName = model.FullName, username = model.Username, role };
            if (errors.Count > 0)
                return BackToUsers("add", errors, values);

            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Role = role!,
                RecoveryEmail = null,
                SecurityStamp = AccountController.NewSecurityStamp()
            };
            // I-hash ang password.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.Password!);
            _context.UserAccounts.Add(account);

            if (!TrySave(errors))
                return BackToUsers("add", errors, values);

            TempData[NoticeKey] = $"{account.Role} account \"{account.FullName}\" was created.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAccount([Bind(nameof(SuperAdminAccountFormViewModel.Id), nameof(SuperAdminAccountFormViewModel.FullName),
            nameof(SuperAdminAccountFormViewModel.Username), nameof(SuperAdminAccountFormViewModel.Role))] SuperAdminAccountFormViewModel model)
        {
            var account = ManagedAccount(model.Id);
            if (account == null)
                return AccountNotAvailable();

            var errors = ModelErrors();
            // Dili pwede himuon nga SuperAdmin.
            var role = AssignableRole(model.Role, errors);
            ValidateAccountFields(model.FullName, model.Username, exceptId: account.Id, errors);

            var values = new { id = account.Id, fullName = model.FullName, username = model.Username, role };
            if (errors.Count > 0)
                return BackToUsers("edit", errors, values);

            account.FullName = model.FullName.Trim();
            account.Username = model.Username.Trim();
            account.Role = role!;

            if (!TrySave(errors))
                return BackToUsers("edit", errors, values);

            TempData[NoticeKey] = $"Account \"{account.FullName}\" was updated.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAccount(int id)
        {
            // Dili ma-delete ang SuperAdmin (bisan gi-forge ang request).
            var account = ManagedAccount(id);
            if (account == null)
                return AccountNotAvailable();

            // Login account ra; ang patient records dili apil.
            _context.UserAccounts.Remove(account);
            _context.SaveChanges();

            TempData[NoticeKey] = $"{account.Role} account \"{account.FullName}\" was deleted.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetAccountPassword(ResetAccountPasswordViewModel model)
        {
            var account = ManagedAccount(model.Id);
            if (account == null)
                return AccountNotAvailable();

            var errors = new Dictionary<string, string>();
            var passwordError = AccountController.BasicPasswordError(model.NewPassword);
            if (passwordError != null)
                errors[nameof(model.NewPassword)] = passwordError;
            if (string.IsNullOrEmpty(model.ConfirmPassword))
                errors[nameof(model.ConfirmPassword)] = "Kinahanglan kini nga field.";
            else if (model.ConfirmPassword != model.NewPassword)
                errors[nameof(model.ConfirmPassword)] = "Dili parehas ang password.";

            if (errors.Count > 0)
                return BackToUsers("reset", errors, new { id = account.Id, fullName = account.FullName });

            // Temporary password: usbon sa sunod nga login; ang daan nga session mawala.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.NewPassword!);
            account.MustChangePassword = true;
            account.SecurityStamp = AccountController.NewSecurityStamp();
            account.FailedLoginAttempts = 0;
            account.LockoutEndUtc = null;
            _context.SaveChanges();

            TempData[NoticeKey] = $"Password reset for \"{account.FullName}\". They must change it at their next sign-in.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // =======================================================================
        // Settings (the signed-in SuperAdmin only)
        // =======================================================================

        [HttpGet]
        public IActionResult Settings()
        {
            var account = CurrentSuperAdmin();
            if (account == null)
                return Redirect(LoginPath);

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
                return Redirect(LoginPath);

            var errors = new Dictionary<string, string>();
            CheckCurrentPassword(account, model.CurrentPassword, errors);

            var username = model.NewUsername?.Trim() ?? string.Empty;
            if (username.Length == 0)
                errors[nameof(model.NewUsername)] = "Kinahanglan kini nga field.";
            else if (!UsernamePattern.IsMatch(username))
                errors[nameof(model.NewUsername)] = "3-50 ka letra, numero, . _ - lang.";
            else if (string.Equals(username, account.Username, StringComparison.OrdinalIgnoreCase))
                errors[nameof(model.NewUsername)] = "Parehas ra sa karon nga username.";
            else if (_context.UserAccounts.Any(u => u.Username == username && u.Id != account.Id))
                errors[nameof(model.NewUsername)] = "Gigamit na kini nga username.";

            if (errors.Count > 0)
                return BackToSettings("username", errors, new { newUsername = model.NewUsername });

            account.Username = username;
            if (!TrySave(errors, nameof(model.NewUsername)))
                return BackToSettings("username", errors, new { newUsername = model.NewUsername });

            // I-renew ang claims (bag-o nga username).
            await HttpContext.SignInAsync(Scheme, CreateSuperAdminPrincipal(account, SignedInAt(User) ?? DateTime.UtcNow));
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
                return Redirect(LoginPath);

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
            await HttpContext.SignInAsync(Scheme, CreateSuperAdminPrincipal(account, SignedInAt(User) ?? DateTime.UtcNow));
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
                return Redirect(LoginPath);

            var errors = new Dictionary<string, string>();
            CheckCurrentPassword(account, model.CurrentPassword, errors);

            var email = NormalizeEmail(model.RecoveryEmail);
            if (email.Length == 0)
                errors[nameof(model.RecoveryEmail)] = "Kinahanglan kini nga field.";
            else if (!IsValidEmail(email))
                errors[nameof(model.RecoveryEmail)] = "Dili valid ang Gmail.";

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
                ModelState.AddModelError(nameof(model.Email), email.Length == 0 ? "Kinahanglan kini nga field." : "Dili valid ang Gmail.");
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
                ModelState.AddModelError(nameof(model.Code), "8 ka numero ang code.");
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

        // SuperAdmin cookie: account, role, stamp ug oras sa login.
        [NonAction]
        public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var account = int.TryParse(principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var accountId)
                ? await db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(u => u.Id == accountId)
                : null;
            var signedInAt = SignedInAt(principal);

            // SuperAdmin ra, parehas nga stamp, ug dili lapas sa 4 ka oras.
            if (principal == null || account == null || account.Role != UserRoles.SuperAdmin
                || !AccountController.StampMatches(principal, account)
                || signedInAt == null || DateTime.UtcNow - signedInAt.Value > MaxSessionAge)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(Scheme);
                return;
            }

            if (principal.FindFirst(ClaimTypes.Name)?.Value != account.Username
                || principal.FindFirst(ClaimTypes.GivenName)?.Value != account.FullName)
            {
                context.ReplacePrincipal(CreateSuperAdminPrincipal(account, signedInAt.Value));
                context.ShouldRenew = true;
            }
        }

        // Admin/Staff nga naka-login: Access Denied. Wala naka-login: SuperAdmin login.
        [NonAction]
        public static async Task RedirectToLoginAsync(RedirectContext<CookieAuthenticationOptions> context)
        {
            var clinicUser = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect(clinicUser.Succeeded ? "/Account/AccessDenied" : LoginPath);
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
            if (string.IsNullOrWhiteSpace(password)) return "Kinahanglan kini nga field.";
            if (password.Length < 12) return "Labing menos 12 ka karakter.";
            if (password.Length > 100) return "Hangtod 100 ka karakter lang.";
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit)
                || !password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                return "Gamiti og dako ug gamay nga letra, numero, ug simbolo.";
            return null;
        }

        // =======================================================================
        // Helpers
        // =======================================================================

        private static ClaimsPrincipal CreateSuperAdminPrincipal(UserAccount account, DateTime signedInUtc)
        {
            var principal = AccountController.CreatePrincipal(account, Scheme);
            ((ClaimsIdentity)principal.Identity!).AddClaim(
                new Claim(SignedInClaim, signedInUtc.Ticks.ToString(CultureInfo.InvariantCulture)));
            return principal;
        }

        private static DateTime? SignedInAt(ClaimsPrincipal? principal) =>
            long.TryParse(principal?.FindFirst(SignedInClaim)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                && ticks > 0 && ticks <= DateTime.MaxValue.Ticks
                ? new DateTime(ticks, DateTimeKind.Utc)
                : null;

        private UserAccount? CurrentSuperAdmin() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? _context.UserAccounts.FirstOrDefault(u => u.Id == id && u.Role == UserRoles.SuperAdmin)
                : null;

        // Admin o Staff ra (dili SuperAdmin, dili ang kaugalingon).
        private UserAccount? ManagedAccount(int id) =>
            _context.UserAccounts.FirstOrDefault(u => u.Id == id && u.Role != UserRoles.SuperAdmin);

        private IActionResult AccountNotAvailable()
        {
            TempData[ErrorKey] = "That account can't be changed here.";
            return RedirectToAction(nameof(ManageUsers));
        }

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
            ModelState.AddModelError(nameof(VerifyCodeViewModel.Code), InvalidCodeMessage);
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
                ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), "Gamita og bag-o nga password, dili ang karaan.");

            if (string.IsNullOrEmpty(confirmPassword))
                ModelState.AddModelError(nameof(ChangePasswordViewModel.ConfirmPassword), "Kinahanglan kini nga field.");
            else if (confirmPassword != newPassword)
                ModelState.AddModelError(nameof(ChangePasswordViewModel.ConfirmPassword), "Dili parehas ang password.");
        }

        private void CheckCurrentPassword(UserAccount account, string? currentPassword, Dictionary<string, string> errors)
        {
            if (string.IsNullOrEmpty(currentPassword))
                errors["CurrentPassword"] = "Kinahanglan kini nga field.";
            else if (_passwordHasher.VerifyHashedPassword(account, account.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
                errors["CurrentPassword"] = "Sayop ang current password.";
        }

        // Admin o Staff ra ang dawaton (eksakto).
        private static string? AssignableRole(string? requestedRole, Dictionary<string, string> errors)
        {
            var role = UserRoles.Assignable.FirstOrDefault(r => r == requestedRole);
            if (role == null)
                errors[nameof(SuperAdminAccountFormViewModel.Role)] = "Pilia ang Admin o Staff.";
            return role;
        }

        private void ValidateAccountFields(string? fullName, string? username, int? exceptId, Dictionary<string, string> errors)
        {
            if (!string.IsNullOrWhiteSpace(fullName) && !Patient.IsValidPersonName(fullName))
                errors.TryAdd(nameof(UserFormViewModel.FullName), "Dili valid ang ngalan.");

            var trimmed = username?.Trim();
            if (!string.IsNullOrEmpty(trimmed) && _context.UserAccounts.Any(u => u.Username == trimmed && u.Id != exceptId))
                errors.TryAdd(nameof(UserFormViewModel.Username), "That username is already taken.");
        }

        private Dictionary<string, string> ModelErrors() =>
            ModelState.Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(entry => entry.Key, entry => entry.Value!.Errors[0].ErrorMessage);

        private bool TrySave(Dictionary<string, string> errors, string usernameField = nameof(UserFormViewModel.Username))
        {
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                errors[usernameField] = "That username is already taken.";
                return false;
            }
        }

        // Balik sa listahan; ablihan pag-usab ang modal (walay password nga ibalik).
        private IActionResult BackToUsers(string form, Dictionary<string, string> errors, object values)
        {
            TempData[FormKey] = JsonSerializer.Serialize(new { form, errors, values });
            return RedirectToAction(nameof(ManageUsers));
        }

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
