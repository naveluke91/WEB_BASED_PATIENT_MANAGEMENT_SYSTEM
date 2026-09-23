using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Marks that a signed-in account has already seen the appointment-reminder
    /// notification for one Appointment. The Appointment itself is never
    /// changed by this — only whether it still shows as unread for that account.
    /// </summary>
    [Table("NotificationReads")]
    public class NotificationRead
    {
        public int Id { get; set; }

        public int UserAccountId { get; set; }

        public int AppointmentId { get; set; }

        public DateTime ReadAtUtc { get; set; }
    }
}
