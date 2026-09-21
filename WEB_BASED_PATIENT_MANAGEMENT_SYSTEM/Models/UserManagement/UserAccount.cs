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

        // UserRoles.SuperAdmin, UserRoles.Admin or UserRoles.Staff
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = UserRoles.Staff;

        // Recovery Gmail sa SuperAdmin ra; NULL sa Admin ug Staff.
        [MaxLength(256)]
        public string? RecoveryEmail { get; set; }

        // Kinahanglan usbon ang password sa sunod nga login.
        public bool MustChangePassword { get; set; }

        // Lockout sa SuperAdmin login.
        public int FailedLoginAttempts { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        // Hash ra sa recovery code, dili ang code mismo.
        [MaxLength(256)]
        public string? PasswordResetCodeHash { get; set; }

        public DateTime? PasswordResetCodeExpiresUtc { get; set; }

        public int PasswordResetFailedAttempts { get; set; }

        // Mausab kung mausab ang password, aron ma-logout ang daan nga session.
        [Required]
        [MaxLength(64)]
        public string SecurityStamp { get; set; } = string.Empty;
    }

    /// <summary>
    /// The application roles. Staff covers midwives and other healthcare staff.
    /// There is only one SuperAdmin; it manages the Admin and Staff accounts.
    /// </summary>
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Staff = "Staff";

        // Mga role nga ma-pili sa SuperAdmin (dili SuperAdmin).
        public static readonly string[] Assignable = { Admin, Staff };
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
        public const string PasswordMin8 = "Use at least 8 characters.";
        public const string PasswordMin12 = "Use at least 12 characters.";
        public const string PasswordMax = "Use 100 characters or fewer.";
        public const string PasswordComplexity = "Use uppercase and lowercase letters, a number, and a symbol.";
        public const string NewPasswordNotCurrent = "Choose a new password, not your current one.";
        public const string NewPasswordNotTemporary = "Choose a new password, not the temporary one.";
        public const string CurrentPasswordIncorrect = "Current password is incorrect.";
        public const string SelectRole = "Select Admin or Staff.";
        public const string AccountNotFound = "That user account no longer exists.";
        public const string ResetStaffOnly = "You can only reset passwords for Staff accounts.";
        public const string ResetAdminStaffOnly = "You can only reset passwords for Admin and Staff accounts.";
        public const string InvalidGmail = "Enter a valid Gmail address.";
        public const string CodeFormat = "The recovery code must be 8 digits.";

        // Usa ra ka mensahe (expired o sayop) aron dili mahibaw-an kung naa ba ang Gmail.
        public const string CodeInvalid = "Invalid or expired recovery code.";
    }
}
