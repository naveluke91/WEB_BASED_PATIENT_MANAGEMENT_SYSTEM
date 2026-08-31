using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Usa ka service nga gipili sa usa ka registered patient.
    /// </summary>
    [Table("Service")]
    public class Service
    {
        /// <summary>
        /// Unique identifier sa service record, awtomatikong gi-generate sa database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key sa patient nga mipili sa service.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        /// <summary>
        /// Pangalan sa napiling service, pananglitan Prenatal o Anti-Tetanus Injection.
        /// </summary>
        [Required(ErrorMessage = "Service name is required.")]
        [MaxLength(150, ErrorMessage = "Service name must be 150 characters or less.")]
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Presyo sa napiling service.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }
}
