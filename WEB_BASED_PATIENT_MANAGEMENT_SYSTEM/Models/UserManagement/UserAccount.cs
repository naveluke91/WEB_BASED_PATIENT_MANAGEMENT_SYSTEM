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
}
