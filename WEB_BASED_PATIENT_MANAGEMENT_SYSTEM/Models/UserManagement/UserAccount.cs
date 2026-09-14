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
    }

    /// <summary>
    /// The two application roles. Staff covers midwives and other healthcare staff.
    /// </summary>
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";

        public static readonly string[] All = { Admin, Staff };
    }
}
