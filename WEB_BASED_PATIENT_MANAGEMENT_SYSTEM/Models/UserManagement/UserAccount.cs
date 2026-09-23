using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// An account that can sign in to the system (Admin or Staff/Midwife).
    /// The password is stored only as a salted hash made by ASP.NET Core's
    /// PasswordHasher, never as plain text.
    /// </summary>
    [Table("UserAccounts")]
    public class UserAccount
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        // Sign-in name; unique (index in ApplicationDbContext).
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        // UserRoles.Admin or UserRoles.Staff
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = UserRoles.Staff;

        // Registered account email used for password recovery. The existing
        // column name is retained so deployed databases do not need a rename.
        [MaxLength(256)]
        public string? RecoveryEmail { get; set; }

        // True only after the account holder proved access to RecoveryEmail by
        // entering a MailKit-sent verification code. Forgot Password only ever
        // uses a verified RecoveryEmail.
        public bool IsRecoveryEmailVerified { get; set; }

        // Indicates whether the account must change its password before using the system.
        public bool MustChangePassword { get; set; }

        // Stores only a hash of the verification code, never the code itself.
        [MaxLength(256)]
        public string? PasswordResetCodeHash { get; set; }

        public DateTime? PasswordResetCodeExpiresUtc { get; set; }

        public int PasswordResetFailedAttempts { get; set; }

        // Changes when the password changes to invalidate existing sessions.
        [Required]
        [MaxLength(64)]
        public string SecurityStamp { get; set; } = string.Empty;
    }

    /// <summary>
    /// The application roles. Staff covers midwives and other healthcare staff.
    /// </summary>
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";

        public static readonly string[] All = { Admin, Staff };
    }

    /// <summary>
    /// Validation messages shown to the user by sign-in and account management (English).
    /// </summary>
    public static class AuthMessages
    {
        public const string Required = "This field is required.";
        public const string InvalidName = "Enter a valid name (letters, spaces, . ' and - only).";
        public const string NameTooLong = "Use 150 characters or fewer.";
        public const string PasswordMismatch = "Passwords do not match.";
        public const string UsernameInUse = "This username is already in use.";
        public const string UsernameLength = "Username must be 3 to 50 characters.";
        public const string UsernameChars = "Username can only use letters, numbers, dots, dashes and underscores.";
        public const string SameUsername = "This is already your username.";
        public const string EmailRequired = "Email address is required.";
        public const string InvalidEmail = "Enter a valid email address.";
        public const string PasswordMin8 = "Use at least 8 characters.";
        public const string PasswordMax = "Use 100 characters or fewer.";
        public const string NewPasswordNotCurrent = "Choose a new password, not your current one.";
        public const string NewPasswordNotTemporary = "Choose a new password, not the temporary one.";
        public const string AccountNotFound = "That user account no longer exists.";
        public const string ResetStaffOnly = "You can only reset passwords for Staff accounts.";
        public const string NoVerifiedRecoveryEmail = "No verified recovery email is registered for this account.";
    }
}
