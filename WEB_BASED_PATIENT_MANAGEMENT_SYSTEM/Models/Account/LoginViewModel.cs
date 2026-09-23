using System.ComponentModel.DataAnnotations;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Local page to return to after signing in.
        public string? ReturnUrl { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }

    public class ResetAccountPasswordViewModel
    {
        public int Id { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3 to 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Username can only use letters, numbers, dots, dashes and underscores.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256, ErrorMessage = "Email address must be 256 characters or fewer.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyPasswordResetCodeViewModel
    {
        [Required(ErrorMessage = "Verification code is required.")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Invalid verification code.")]
        [Display(Name = "Verification Code")]
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmPassword { get; set; }
    }
}
