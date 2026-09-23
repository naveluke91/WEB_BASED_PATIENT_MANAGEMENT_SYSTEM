using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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

        // ---- Validation (Patients ug Appointments) ----

        // Mga field nga gikan sa patient form.
        public const string FormFields = "FullName,Address,DateOfBirth,MaritalStatus,Religion,Occupation,ContactNo,LMP,AOG,EDC,Menarche,Gravida,TFAL";

        // Mga pilianan sa marital status.
        public static readonly string[] MaritalStatuses = { "Single", "Married", "Widowed", "Separated" };

        private const string RequiredMessage = "This field is required.";

        private static readonly Regex NamePattern = new(@"^[\p{L}\p{M} .'’\-]+$");
        private static readonly Regex ContactPattern = new(@"^(09\d{9}|\+639\d{9})$");
        private static readonly Regex TextPattern = new(@"^(?=.*\p{L})[\p{L}\p{M}\p{N} .,'’()&/\-]+$");
        private static readonly Regex AogPattern = new(@"^(\d{1,2})(\.\d{1,2})?\s*(weeks?|wks?|w)?(\s*(and\s+)?[0-6]\s*(days?|d))?$", RegexOptions.IgnoreCase);
        private static readonly Regex WholeNumberPattern = new(@"^\d{1,2}$");
        private static readonly Regex TfalPattern = new(@"^\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}$");

        // Letra, espasyo, . ' - lang; labing menos 2 ka letra.
        public static bool IsValidPersonName(string? value) =>
            !string.IsNullOrWhiteSpace(value)
            && NamePattern.IsMatch(value.Trim())
            && value.Count(char.IsLetter) >= 2;

        // 09XXXXXXXXX o +639XXXXXXXXX lang.
        public static bool IsValidContactNo(string? value) =>
            !string.IsNullOrWhiteSpace(value) && ContactPattern.IsMatch(value.Trim());

        // I-trim ang mga text nga gi-input.
        public void TrimTextFields()
        {
            FullName = FullName?.Trim() ?? string.Empty;
            Address = Address?.Trim() ?? string.Empty;
            MaritalStatus = MaritalStatus?.Trim() ?? string.Empty;
            Religion = Religion?.Trim();
            Occupation = Occupation?.Trim();
            ContactNo = ContactNo?.Trim() ?? string.Empty;
            AOG = AOG?.Trim();
            Menarche = Menarche?.Trim();
            Gravida = Gravida?.Trim();
            TFAL = TFAL?.Trim();
        }

        // I-validate ang patient form; ibalik ang sayop matag field.
        public static Dictionary<string, string> ValidateInput(Patient patient)
        {
            var errors = new Dictionary<string, string>();
            var today = DateTime.Today;

            void Fail(string field, string message) => errors.TryAdd(field, message);

            // Required nga text: dili blangko, dili lapas sa max.
            bool CheckRequired(string? value, string field, int maxLength)
            {
                if (string.IsNullOrWhiteSpace(value)) { Fail(field, RequiredMessage); return false; }
                if (value.Length > maxLength) { Fail(field, $"Use {maxLength} characters or fewer."); return false; }
                return true;
            }

            if (CheckRequired(patient.FullName, nameof(FullName), 150) && !IsValidPersonName(patient.FullName))
                Fail(nameof(FullName), "Enter a valid name.");

            if (CheckRequired(patient.Address, nameof(Address), 250) && !patient.Address.Any(char.IsLetterOrDigit))
                Fail(nameof(Address), "Enter a valid address.");

            var dateOfBirth = patient.DateOfBirth?.Date;
            if (!dateOfBirth.HasValue) Fail(nameof(DateOfBirth), RequiredMessage);
            else if (dateOfBirth > today) Fail(nameof(DateOfBirth), "Date cannot be in the future.");
            else if (CalculateAge(dateOfBirth.Value) > 130) Fail(nameof(DateOfBirth), "Age cannot be more than 130 years.");
            bool dateOfBirthIsValid = dateOfBirth.HasValue && !errors.ContainsKey(nameof(DateOfBirth));

            if (string.IsNullOrWhiteSpace(patient.MaritalStatus)) Fail(nameof(MaritalStatus), RequiredMessage);
            else if (!MaritalStatuses.Contains(patient.MaritalStatus)) Fail(nameof(MaritalStatus), "Please select a valid option.");

            if (CheckRequired(patient.Religion, nameof(Religion), 100) && !TextPattern.IsMatch(patient.Religion!))
                Fail(nameof(Religion), "Enter a valid religion.");

            if (CheckRequired(patient.Occupation, nameof(Occupation), 100) && !TextPattern.IsMatch(patient.Occupation!))
                Fail(nameof(Occupation), "Enter a valid occupation.");

            if (string.IsNullOrWhiteSpace(patient.ContactNo)) Fail(nameof(ContactNo), "Contact number is required.");
            else if (!IsValidContactNo(patient.ContactNo)) Fail(nameof(ContactNo), "Contact number must contain 11 digits.");

            var lmp = patient.LMP?.Date;
            if (!lmp.HasValue) Fail(nameof(LMP), RequiredMessage);
            else if (lmp > today) Fail(nameof(LMP), "Date cannot be in the future.");
            else if (dateOfBirthIsValid && lmp < dateOfBirth) Fail(nameof(LMP), "Date cannot be before the date of birth.");
            bool lmpIsValid = lmp.HasValue && !errors.ContainsKey(nameof(LMP));

            if (CheckRequired(patient.AOG, nameof(AOG), 50))
            {
                var aog = AogPattern.Match(patient.AOG!.Trim());
                if (!aog.Success || int.Parse(aog.Groups[1].Value) > 45)
                    Fail(nameof(AOG), "Enter a valid AOG (example: 14 weeks).");
            }

            var edc = patient.EDC?.Date;
            if (!edc.HasValue) Fail(nameof(EDC), RequiredMessage);
            else if (lmpIsValid && edc <= lmp) Fail(nameof(EDC), "Date must be after the LMP.");
            else if (lmpIsValid && edc > lmp!.Value.AddDays(315)) Fail(nameof(EDC), "Date is too far after the LMP.");

            if (CheckRequired(patient.Menarche, nameof(Menarche), 50))
            {
                var menarche = patient.Menarche!.Trim();
                if (!WholeNumberPattern.IsMatch(menarche) || int.Parse(menarche) < 5 || int.Parse(menarche) > 30)
                    Fail(nameof(Menarche), "Enter a valid age at menarche.");
                else if (dateOfBirthIsValid && int.Parse(menarche) > CalculateAge(dateOfBirth!.Value))
                    Fail(nameof(Menarche), "Cannot be more than the patient's age.");
            }

            if (CheckRequired(patient.Gravida, nameof(Gravida), 50) && !WholeNumberPattern.IsMatch(patient.Gravida!.Trim()))
                Fail(nameof(Gravida), "Enter a whole number from 0 to 99.");

            if (CheckRequired(patient.TFAL, nameof(TFAL), 50) && !TfalPattern.IsMatch(patient.TFAL!.Trim()))
                Fail(nameof(TFAL), "Use the format 2-1-0-1.");

            return errors;
        }
    }
}
