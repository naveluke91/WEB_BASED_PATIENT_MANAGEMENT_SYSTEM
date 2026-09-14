using System.Security.Claims;
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
    /// Sign in and sign out.
    /// Switched off for now with [NonController]: the system runs without sign-in,
    /// so /Account/Login and /Account/Setup don't exist. Remove the attribute in the
    /// later Admin/Staff sign-in task.
    /// </summary>
    [NonController]
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
            var account = _context.UserAccounts.FirstOrDefault(u => u.Username == username);
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
            return RedirectToLocal(model.ReturnUrl);
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
            if (_context.UserAccounts.Any())
                return RedirectToAction(nameof(Login));

            return View(new UserFormViewModel { Role = UserRoles.Admin });
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
                return RedirectToAction(nameof(Login));

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            if (!ModelState.IsValid)
                return View(model);

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
        public static ClaimsPrincipal CreatePrincipal(UserAccount account)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username),
                new(ClaimTypes.GivenName, account.FullName),
                new(ClaimTypes.Role, account.Role)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        // Only local return URLs are followed, so a link cannot send the user elsewhere.
        private IActionResult RedirectToLocal(string? returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Patients");
    }
}
