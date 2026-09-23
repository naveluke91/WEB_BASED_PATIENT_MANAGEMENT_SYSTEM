using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// One payment transaction recorded for a completed consultation.
    /// Patient and service details are read through Consultation → Patient,
    /// not copied into this table.
    /// </summary>
    [Table("Payments")]
    public class Payment
    {
        public int Id { get; set; }

        // The consultation being paid for. ConsultationId is unique
        // (ApplicationDbContext), so a consultation has at most one payment.
        [Required]
        [Display(Name = "Consultation")]
        public int ConsultationId { get; set; }
        public Consultation? Consultation { get; set; }

        // Nullable only so a blank amount shows "Amount is required."
        // (same pattern as Patient.DateOfBirth); the column is NOT NULL.
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, 99999999.99, ErrorMessage = "Amount must be between 0.00 and 99,999,999.99.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Amount { get; set; }

        // Cash or GCash (validated in BillingController.Create).
        [Required(ErrorMessage = "Payment method is required.")]
        [MaxLength(30)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
