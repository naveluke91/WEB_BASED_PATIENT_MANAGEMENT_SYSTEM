using System.Text.Json;
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
    /// SuperAdmin manages Admin and Staff accounts; Admin manages Staff only.
    /// </summary>
    // SuperAdmin ug Admin ra maka-access (Staff → Access Denied).
    [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
    public class UserManagementController : Controller
    {
        private const string NotAllowedMessage = "You can't manage that account.";

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;

        public UserManagementController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // I-check ang role sa server: ang SuperAdmin dili gikan sa browser.
        private bool IsSuperAdmin => User.IsInRole(UserRoles.SuperAdmin);

        // SuperAdmin: Admin ug Staff. Admin: Staff ra. Ang SuperAdmin dili ma-manage.
        private string[] ManagedRoles => IsSuperAdmin ? new[] { UserRoles.Admin, UserRoles.Staff } : new[] { UserRoles.Staff };

        private UserAccount? ManagedAccount(int id)
        {
            var roles = ManagedRoles;
            return _context.UserAccounts.FirstOrDefault(u => u.Id == id && roles.Contains(u.Role));
        }

        // -----------------------------------------------------------------------
        // GET /UserManagement
        // -----------------------------------------------------------------------
        public IActionResult Index()
        {
            var roles = ManagedRoles;
            var users = _context.UserAccounts
                .AsNoTracking()
                .Where(u => roles.Contains(u.Role))
                .OrderBy(u => u.FullName)
                .ToList();

            return View(new UserManagementPageViewModel { Users = users, IsSuperAdmin = IsSuperAdmin });
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Create
        // SuperAdmin chooses Admin or Staff; Admin always creates Staff.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(nameof(UserFormViewModel.FullName), nameof(UserFormViewModel.Username),
            nameof(UserFormViewModel.Password), nameof(UserFormViewModel.ConfirmPassword), nameof(UserFormViewModel.Role))] UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            // Admin: Staff kanunay, ang Role gikan sa browser dili gamiton.
            // SuperAdmin: Admin o Staff ra (eksakto); ang SuperAdmin i-reject.
            var role = UserRoles.Staff;
            if (IsSuperAdmin)
            {
                var chosen = UserRoles.Assignable.FirstOrDefault(r => r == model.Role);
                if (chosen == null)
                    ModelState.AddModelError(nameof(UserFormViewModel.Role), AuthMessages.SelectRole);
                else
                    role = chosen;
            }
            else
            {
                model.Role = null;
            }

            ValidateUsernameIsFree(model.Username, exceptId: null);
            ValidateFullName(model.FullName);

            if (!ModelState.IsValid)
                return ReopenForm("add", model);

            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Role = role,
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
        // Updates the name and username (and the Admin/Staff role, SuperAdmin only).
        // The password is not changed here.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind(nameof(UserFormViewModel.Id), nameof(UserFormViewModel.FullName),
            nameof(UserFormViewModel.Username), nameof(UserFormViewModel.Role))] UserFormViewModel model)
        {
            // Ang account nga ma-manage ra (dili SuperAdmin; ang Admin dili makausab sa Admin).
            var account = ManagedAccount(model.Id);
            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
                return RedirectToAction(nameof(Index));
            }

            // Ang Role sa database dili usbon gawas kung SuperAdmin ug Admin/Staff ang gipili.
            var role = account.Role;
            if (IsSuperAdmin)
            {
                var chosen = UserRoles.Assignable.FirstOrDefault(r => r == model.Role);
                if (chosen == null)
                    ModelState.AddModelError(nameof(UserFormViewModel.Role), AuthMessages.SelectRole);
                else
                    role = chosen;
            }
            else
            {
                model.Role = null;
            }

            ValidateUsernameIsFree(model.Username, exceptId: account.Id);
            ValidateFullName(model.FullName);

            if (!ModelState.IsValid)
                return ReopenForm("edit", model);

            account.FullName = model.FullName.Trim();
            account.Username = model.Username.Trim();
            account.Role = role;

            if (!TrySave())
                return ReopenForm("edit", model);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/ResetPassword
        // SuperAdmin only: sets a temporary password for an Admin or Staff account.
        // -----------------------------------------------------------------------
        // SuperAdmin ra ang maka-reset.
        [Authorize(Roles = UserRoles.SuperAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetAccountPasswordViewModel model)
        {
            var account = ManagedAccount(model.Id);
            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
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

            // I-hash ang temporary password; usbon sa sunod nga login, ug mawala ang daan nga session.
            account.PasswordHash = _passwordHasher.HashPassword(account, model.NewPassword!);
            account.MustChangePassword = true;
            account.SecurityStamp = AccountController.NewSecurityStamp();
            account.FailedLoginAttempts = 0;
            account.LockoutEndUtc = null;
            _context.SaveChanges();

            TempData["UserNotice"] = $"Password reset for \"{account.FullName}\". They must change it at their next sign-in.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Delete/5
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Dili ma-delete ang SuperAdmin; ang Admin makadelete sa Staff ra.
            var account = ManagedAccount(id);

            if (account == null)
            {
                TempData["ErrorMessage"] = NotAllowedMessage;
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
                role = model.Role,
                error,
                // Ang field nga may sayop (para ma-marka og pula).
                field = ModelState.FirstOrDefault(entry => entry.Value?.Errors.Count > 0).Key
            });

            return RedirectToAction(nameof(Index));
        }
    }
}
