using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// Sign in and sign out. Admin and Staff use the same Login; the Role saved in
    /// UserAccounts decides what the account can open.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
            // Admin ug Staff ra; ang SuperAdmin naa sa lahi nga login.
            var account = _context.UserAccounts.FirstOrDefault(u => u.Username == username && u.Role != UserRoles.SuperAdmin);
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

            // Temporary password: usbon una una makasulod.
            if (account.MustChangePassword)
                return RedirectToAction(nameof(ChangePassword));

            return RedirectToLocal(model.ReturnUrl);
        }

        // -----------------------------------------------------------------------
        // GET /Account/ChangePassword
        // Only after the SuperAdmin reset this account's password: the temporary
        // password must be replaced before the system can be used.
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

            // I-hash ang bag-o nga password; bag-o nga stamp = logout sa ubang session.
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
            var wasSuperAdmin = User.IsInRole(UserRoles.SuperAdmin);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Ang SuperAdmin balik sa iyang login; ang uban sa normal nga login.
            return wasSuperAdmin ? Redirect(SuperAdminController.LoginPath) : RedirectToAction(nameof(Login));
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
            // Naa nay account: dili na pwede mag-himo og laing Admin.
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
            nameof(UserFormViewModel.Password), nameof(UserFormViewModel.ConfirmPassword))] UserFormViewModel model)
        {
            if (_context.UserAccounts.Any())
                return SetupClosed();

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            // Valid nga ngalan lang (parehas sa Users).
            if (!string.IsNullOrWhiteSpace(model.FullName) && !Patient.IsValidPersonName(model.FullName))
                ModelState.AddModelError(nameof(UserFormViewModel.FullName), AuthMessages.InvalidName);

            if (!ModelState.IsValid)
                return View(model);

            // Ang una nga account kanunay Admin; dili gikan sa form.
            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
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
        public static ClaimsPrincipal CreatePrincipal(UserAccount account, DateTime? signedInUtc = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username),
                new(ClaimTypes.GivenName, account.FullName),
                new(ClaimTypes.Role, account.Role),
                // Security stamp: mausab kung mausab ang password.
                new(SecurityStampClaim, account.SecurityStamp ?? string.Empty)
            };

            // Oras sa login: gamiton sa SuperAdmin session limit.
            if (signedInUtc.HasValue)
                claims.Add(new Claim(SignedInClaim, signedInUtc.Value.Ticks.ToString(CultureInfo.InvariantCulture)));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        public const string SecurityStampClaim = "security_stamp";
        public const string SignedInClaim = "signed_in_utc";

        // SuperAdmin session: 30 minutos nga walay lihok, 4 ka oras labing dugay.
        public static readonly TimeSpan SuperAdminIdleTimeout = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan SuperAdminMaxSession = TimeSpan.FromHours(4);

        [NonAction]
        public static DateTime? SignedInAt(ClaimsPrincipal? principal) =>
            long.TryParse(principal?.FindFirst(SignedInClaim)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                && ticks > 0 && ticks <= DateTime.MaxValue.Ticks
                ? new DateTime(ticks, DateTimeKind.Utc)
                : null;

        // Cookie sa SuperAdmin: mubo ang idle timeout kaysa sa clinic.
        [NonAction]
        public static AuthenticationProperties SuperAdminProperties() =>
            new() { IsPersistent = false, AllowRefresh = true, ExpiresUtc = DateTimeOffset.UtcNow.Add(SuperAdminIdleTimeout) };

        // Parehas pa ba ang stamp sa cookie ug sa database?
        [NonAction]
        public static bool StampMatches(ClaimsPrincipal principal, UserAccount account) =>
            (principal.FindFirst(SecurityStampClaim)?.Value ?? string.Empty) == (account.SecurityStamp ?? string.Empty);

        [NonAction]
        public static string NewSecurityStamp() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        // Password sa Admin ug Staff: 8-100 ka karakter, dili puro space.
        [NonAction]
        public static string? BasicPasswordError(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return AuthMessages.Required;
            if (password.Length < 8) return AuthMessages.PasswordMin8;
            if (password.Length > 100) return AuthMessages.PasswordMax;
            return null;
        }

        // Sample nga Staff ug Admin para sa local testing (Development ra; tawagon sa Program.cs).
        // Idempotent: kung naa na ang username, dili himuon ug dili usbon ang password.
        // Ang password i-hash; walay plain text sa database.
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
                        // Test account: pwede mag-login dayon; walay Gmail.
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

        // Ang naka-login nga account gikan sa database.
        private UserAccount? CurrentAccount() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? _context.UserAccounts.FirstOrDefault(u => u.Id == id && u.Role != UserRoles.SuperAdmin)
                : null;

        // Sirado na ang Setup: Patients kung naka-login, Login kung wala.
        private IActionResult SetupClosed() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Patients") : RedirectToAction(nameof(Login));

        // Only local return URLs are followed, so a link cannot send the user elsewhere.
        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Patients");
    }
}
