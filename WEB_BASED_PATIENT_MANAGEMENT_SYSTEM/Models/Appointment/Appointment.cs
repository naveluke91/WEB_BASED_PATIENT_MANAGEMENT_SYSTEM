using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Representasyon sa usa ka appointment sa sistema.
    /// Naglangkob sa petsa, oras, ug impormasyon sa pasyente para sa appointment.
    /// </summary>
    [Table("Appointments")]
    public class Appointment
    {
        /// <summary>
        /// Unique identifier sa appointment — awtomatiko nga gi-generate sa database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key — ang ID sa pasyente nga may appointment.
        /// Pwede nga null kung ang appointment gi-create para sa dili pa naka-register nga pasyente.
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// Navigation property — ang pasyente nga naay appointment.
        /// </summary>
        public Patient? Patient { get; set; }

        /// <summary>
        /// Ngalan sa pasyente kung wala pa ni-register sa sistema.
        /// Gigamit isip fallback kung PatientId is null.
        /// </summary>
        [Required(ErrorMessage = "Patient name is required.")]
        [MaxLength(150, ErrorMessage = "Patient name must be 150 characters or less.")]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// Contact number sa pasyente para sa appointment.
        /// </summary>
        [Required(ErrorMessage = "Contact number is required.")]
        [MaxLength(20, ErrorMessage = "Contact number must be 20 characters or less.")]
        [Display(Name = "Contact No.")]
        public string ContactNo { get; set; } = string.Empty;

        /// <summary>
        /// Petsa sa appointment.
        /// </summary>
        [Required(ErrorMessage = "Appointment date is required.")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Oras sa appointment (e.g. "09:00 AM").
        /// </summary>
        [Required(ErrorMessage = "Appointment time is required.")]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; }

        /// <summary>
        /// Klase sa appointment / serbisyo nga gagamiton (e.g. Prenatal, Family Planning).
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Service Type")]
        public string? ServiceType { get; set; }

        /// <summary>
        /// Dugang impormasyon o notes para sa appointment.
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Status sa appointment: Pending, Confirmed, Cancelled, Rescheduled.
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Indicator kung ang pasyente kay currently in-process / gi-serve na.
        /// </summary>
        [Display(Name = "In Process")]
        public bool InProcess { get; set; } = false;

        /// <summary>
        /// Petsa kung kanus-a gi-create ang appointment record.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Helper property — formatted time para sa display (e.g. "9:00 AM").
        /// </summary>
        [NotMapped]
        public string FormattedTime =>
            DateTime.Today.Add(AppointmentTime).ToString("h:mm tt");
    }
}
