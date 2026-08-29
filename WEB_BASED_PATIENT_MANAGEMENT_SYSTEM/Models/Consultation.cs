using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// One actual clinic encounter for a registered patient. It may originate
    /// from a confirmed appointment or from a walk-in patient.
    /// </summary>
    [Table("Consultations")]
    public class Consultation
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Null only when the patient is a walk-in.
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        [MaxLength(30)]
        public string VisitType { get; set; } = "Walk-In";

        [Required]
        [MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "In Progress";

        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        // Points to the saved service record so the completed consultation can
        // open its existing Details page.
        [MaxLength(30)]
        public string? RecordType { get; set; }
        public int? RecordId { get; set; }
    }
}
