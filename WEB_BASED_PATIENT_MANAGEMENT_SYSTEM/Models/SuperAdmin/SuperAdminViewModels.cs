namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    // Bag-o nga password (forced change human sa temporary password).
    public class ChangePasswordViewModel
    {
        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }

    // SuperAdmin → Add Account: naay Role, pero Admin o Staff ra.
    public class SuperAdminAccountFormViewModel : UserFormViewModel
    {
        public string? Role { get; set; }
    }

    // SuperAdmin → Reset Password sa Admin o Staff.
    public class ResetAccountPasswordViewModel
    {
        public int Id { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }

    // SuperAdmin → Manage Users (Admin ug Staff ra ang listahan).
    public class SuperAdminUsersPageViewModel
    {
        public List<UserAccount> Accounts { get; set; } = new();
    }

    // SuperAdmin → Settings (walay password o hash nga ipakita).
    public class SuperAdminSettingsViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string? RecoveryEmail { get; set; }
    }

    // Settings: usbon ang username.
    public class ChangeUsernameViewModel
    {
        public string? CurrentPassword { get; set; }

        public string? NewUsername { get; set; }
    }

    // Settings: usbon ang password.
    public class ChangeOwnPasswordViewModel
    {
        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }

    // Settings: recovery Gmail.
    public class RecoveryEmailViewModel
    {
        public string? CurrentPassword { get; set; }

        public string? RecoveryEmail { get; set; }
    }

    // Forgot Password: recovery Gmail.
    public class ForgotPasswordViewModel
    {
        public string? Email { get; set; }
    }

    // Verify Recovery Code: 8 ka numero.
    public class VerifyCodeViewModel
    {
        public string? Code { get; set; }
    }

    // Reset Password human sa sakto nga code (ang Token dili ang code).
    public class RecoveryResetViewModel
    {
        public string? Token { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmPassword { get; set; }
    }
}
