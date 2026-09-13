using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Representasyon sa usa ka pasyente sa sistema.
    /// Naglangkob sa personal, obstetric, ug contact information sa matag pasyente.
    /// Kini nga klase mao ang Table nga gitawag og "Patient" sa database.
    /// </summary>
    [Table("Patient")]
    public class Patient
    {
        /// <summary>
        /// Unique identifier sa pasyente — awtomatiko nga gi-generate sa database (IDENTITY).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Buong pangalan sa pasyente (required).
        /// Dili pwede mo-blank, max 150 characters.
        /// </summary>
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        [MaxLength(150, ErrorMessage = "Full name must be 150 characters or less.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Adres sa pasyente (required).
        /// Max 250 characters para kasulod ang dugay nga adres.
        /// </summary>
        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(250, ErrorMessage = "Address must be 250 characters or less.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Edad sa pasyente — gikuha gikan sa DateOfBirth inigpa-save.
        /// Gi-store para dali nga ma-display sa patient list.
        /// </summary>
        [Display(Name = "Age")]
        [Range(0, 130, ErrorMessage = "Age must be between 0 and 130.")]
        public int Age { get; set; }

        /// <summary>
        /// Civil status sa pasyente (e.g. Single, Married, Widowed).
        /// Required — max 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Marital status is required.")]
        [Display(Name = "Marital Status")]
        [MaxLength(50, ErrorMessage = "Marital status must be 50 characters or less.")]
        public string MaritalStatus { get; set; } = string.Empty;

        /// <summary>
        /// Petsa sa pagkatawo sa pasyente (required).
        /// Gigamit para kalkulahon ang edad.
        /// </summary>
        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Relihiyon sa pasyente (e.g. Roman Catholic, INC).
        /// Dili required — max 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Religion is required.")]
        [MaxLength(100, ErrorMessage = "Religion must be 100 characters or less.")]
        public string? Religion { get; set; }

        /// <summary>
        /// Last Menstrual Period — gigamit para sa obstetric tracking.
        /// Gikinahanglan para kalkulahon ang AOG ug EDC.
        /// </summary>
        [Required(ErrorMessage = "Last menstrual period is required.")]
        [Display(Name = "LMP (Last Menstrual Period)")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? LMP { get; set; }

        /// <summary>
        /// Age of Gestation — pila na ka semana ang bata (e.g. "18 weeks").
        /// Max 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Age of gestation is required.")]
        [Display(Name = "AOG (Age of Gestation)")]
        [MaxLength(50, ErrorMessage = "Age of gestation must be 50 characters or less.")]
        public string? AOG { get; set; }

        /// <summary>
        /// Estimated Date of Confinement — gilauman nga petsa sa pag-anak.
        /// </summary>
        [Required(ErrorMessage = "Estimated date of confinement is required.")]
        [Display(Name = "EDC (Estimated Date of Confinement)")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime? EDC { get; set; }

        /// <summary>
        /// Edad kung diin nagsugod ang regla sa pasyente (e.g. "12").
        /// Dili required — max 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Menarche is required.")]
        [MaxLength(50, ErrorMessage = "Menarche must be 50 characters or less.")]
        public string? Menarche { get; set; }

        /// <summary>
        /// Numero sa telepono sa pasyente (required).
        /// Max 20 characters para kasulod ang mobile ug landline.
        /// </summary>
        [Required(ErrorMessage = "Contact number is required.")]
        [Display(Name = "Contact No.")]
        [MaxLength(20, ErrorMessage = "Contact number must be 20 characters or less.")]
        public string ContactNo { get; set; } = string.Empty;

        /// <summary>
        /// Gravida — pila ka beses nagsabak ang pasyente (including current).
        /// Dili required — max 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Gravida is required.")]
        [MaxLength(50, ErrorMessage = "Gravida must be 50 characters or less.")]
        public string? Gravida { get; set; }

        /// <summary>
        /// TFAL — Term, Full-term, Abortions, Living children (e.g. "2-1-0-1").
        /// Dili required — max 50 characters.
        /// </summary>
        [Required(ErrorMessage = "TFAL is required.")]
        [Display(Name = "TFAL (Term, Full-term, Abortions, Living)")]
        [MaxLength(50, ErrorMessage = "TFAL must be 50 characters or less.")]
        public string? TFAL { get; set; }

        /// <summary>
        /// Trabaho sa pasyente (e.g. Nurse, Teacher, Student).
        /// Dili required — max 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Occupation is required.")]
        [MaxLength(100, ErrorMessage = "Occupation must be 100 characters or less.")]
        public string? Occupation { get; set; }

        /// <summary>
        /// Navigation property — lista sa tanan nga prenatal records niining pasyente.
        /// Gigamit sa EF Core para sa one-to-many relationship.
        /// </summary>
        public ICollection<PrenatalRecord> PrenatalRecords { get; set; } = new List<PrenatalRecord>();

        public ICollection<NewbornRecord> NewbornRecords { get; set; } = new List<NewbornRecord>();

        public ICollection<FamilyPlanningRecord> FamilyPlanningRecords { get; set; } = new List<FamilyPlanningRecord>();

        /// <summary>
        /// Lista sa mga service nga gipili sa pasyente.
        /// </summary>
        public ICollection<Service> Services { get; set; } = new List<Service>();

        /// <summary>
        /// Kalkulahon ang edad sa pasyente base sa iyang petsa sa pagkatawo.
        /// Gina-amin ang usa ka tuig kung wala pa ang birthday niining tuiga.
        /// </summary>
        /// <param name="dateOfBirth">Petsa sa pagkatawo sa pasyente</param>
        /// <returns>Edad sa pasyente (integer)</returns>
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            // Bawasan ug usa ka tuig kung wala pa maabut ang birthday niining tuiga
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
