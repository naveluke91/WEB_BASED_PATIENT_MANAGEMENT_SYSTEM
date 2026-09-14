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
}
