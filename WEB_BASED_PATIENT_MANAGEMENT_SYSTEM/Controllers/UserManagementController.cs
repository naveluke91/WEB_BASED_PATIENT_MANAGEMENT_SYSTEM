using System.Security.Claims;
using System.Text.Json;
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
    /// </summary>
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;

        public UserManagementController(ApplicationDbContext context, IPasswordHasher<UserAccount> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // -----------------------------------------------------------------------
        // GET /UserManagement
        // -----------------------------------------------------------------------
        public IActionResult Index()
        {
            var users = _context.UserAccounts
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToList();

            return View(new UserManagementPageViewModel
            {
                Users = users,
                CurrentUserId = CurrentUserId,
                AdminCount = users.Count(u => u.Role == UserRoles.Admin)
            });
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Create
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(nameof(UserFormViewModel.FullName), nameof(UserFormViewModel.Username),
            nameof(UserFormViewModel.Password), nameof(UserFormViewModel.ConfirmPassword), nameof(UserFormViewModel.Role))] UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserFormViewModel.Password), "Password is required.");

            var role = ValidateRole(model.Role);
            ValidateUsernameIsFree(model.Username, exceptId: null);
            ValidateFullName(model.FullName);

            if (!ModelState.IsValid)
                return ReopenForm("add", model);

            var account = new UserAccount
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Role = role!
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
        // Updates the name, username and role. The password is not changed here.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind(nameof(UserFormViewModel.Id), nameof(UserFormViewModel.FullName),
            nameof(UserFormViewModel.Username), nameof(UserFormViewModel.Role))] UserFormViewModel model)
        {
            var account = _context.UserAccounts.FirstOrDefault(u => u.Id == model.Id);
            if (account == null)
            {
                TempData["ErrorMessage"] = "That user account no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            var role = ValidateRole(model.Role);
            ValidateUsernameIsFree(model.Username, exceptId: account.Id);
            ValidateFullName(model.FullName);

            if (role != null && role != account.Role)
            {
                if (account.Id == CurrentUserId)
                {
                    ModelState.AddModelError(nameof(UserFormViewModel.Role), "You can't change the role of the account you are signed in with.");
                    model.Role = account.Role;
                }
                else if (account.Role == UserRoles.Admin && AdminCount() <= 1)
                {
                    ModelState.AddModelError(nameof(UserFormViewModel.Role), "The system needs at least one Admin account.");
                }
            }

            if (!ModelState.IsValid)
                return ReopenForm("edit", model);

            account.FullName = model.FullName.Trim();
            account.Username = model.Username.Trim();
            account.Role = role!;

            if (!TrySave())
                return ReopenForm("edit", model);

            TempData["SuccessMessage"] = $"User \"{account.FullName}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /UserManagement/Delete/5
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var account = _context.UserAccounts.FirstOrDefault(u => u.Id == id);

            if (account == null)
            {
                TempData["ErrorMessage"] = "That user account no longer exists.";
            }
            else if (account.Id == CurrentUserId)
            {
                TempData["ErrorMessage"] = "You can't delete the account you are signed in with.";
            }
            else if (account.Role == UserRoles.Admin && AdminCount() <= 1)
            {
                TempData["ErrorMessage"] = "The system needs at least one Admin account.";
            }
            else
            {
                _context.UserAccounts.Remove(account);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"User \"{account.FullName}\" was deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Id of the signed-in Admin (NameIdentifier claim set by AccountController).
        private int CurrentUserId =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        private int AdminCount() =>
            _context.UserAccounts.Count(u => u.Role == UserRoles.Admin);

        // Accepts only Admin or Staff and returns the canonical spelling.
        // A blank role is already reported by the [Required] attribute.
        private string? ValidateRole(string? requestedRole)
        {
            var role = UserRoles.All.FirstOrDefault(r =>
                string.Equals(r, requestedRole?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (role == null && !string.IsNullOrWhiteSpace(requestedRole))
                ModelState.AddModelError(nameof(UserFormViewModel.Role), "Please select Admin or Staff.");

            return role;
        }

        // Valid nga ngalan lang (parehas sa Patient).
        private void ValidateFullName(string? fullName)
        {
            if (!string.IsNullOrWhiteSpace(fullName) && !Patient.IsValidPersonName(fullName))
                ModelState.AddModelError(nameof(UserFormViewModel.FullName), "Dili valid ang ngalan.");
        }

        private void ValidateUsernameIsFree(string? requestedUsername, int? exceptId)
        {
            var username = requestedUsername?.Trim();
            if (string.IsNullOrEmpty(username))
                return;

            if (_context.UserAccounts.Any(u => u.Username == username && u.Id != exceptId))
                ModelState.AddModelError(nameof(UserFormViewModel.Username), "That username is already taken.");
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
                ModelState.AddModelError(nameof(UserFormViewModel.Username), "That username is already taken.");
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
