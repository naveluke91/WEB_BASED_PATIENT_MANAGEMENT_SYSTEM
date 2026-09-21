using System.ComponentModel.DataAnnotations;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Add User, Edit User and first Admin setup form. The password fields are
    /// only used when an account is created.
    /// </summary>
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150, ErrorMessage = "Full name must be 150 characters or less.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3 to 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Username can only use letters, numbers, dots, dashes and underscores.")]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        // SuperAdmin ra ang mogamit niini (Admin o Staff). Ang Admin ug ang Setup
        // dili mogamit: Add User = Staff, Setup = Admin, ang server ang mo-set.
        public string? Role { get; set; }
    }
}
