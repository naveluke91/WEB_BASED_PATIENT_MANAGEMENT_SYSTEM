using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Representasyon sa usa ka prenatal checkup record sa sistema.
    /// Matag pasyente mahimong naa'y daghang prenatal records (one-to-many).
    /// Kini nga klase mao ang Table nga gitawag og "PrenatalRecords" sa database.
    /// </summary>
    [Table("PrenatalRecords")]
    public class PrenatalRecord
    {
        /// <summary>
        /// Unique identifier sa prenatal record — awtomatiko nga gi-generate sa database (IDENTITY).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key — nagpakita kung kinsa nga pasyente ang tag-iya niining prenatal record.
        /// Kinahanglan (required) — dili pwede mag-save ug prenatal record nga walay pasyente.
        /// </summary>
        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int? PatientId { get; set; }

        /// <summary>
        /// Navigation property — ang Patient nga tag-iya niining record.
        /// Gigamit sa EF Core para ma-load ang datos sa pasyente.
        /// </summary>
        public Patient? Patient { get; set; }

        /// <summary>
        /// Petsa sa checkup (required).
        /// Default: petsa karon (DateTime.Today).
        /// </summary>
        [Required(ErrorMessage = "Checkup date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Checkup Date")]
        [Column(TypeName = "date")]
        public DateTime? RecordDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Age of Gestation — pila ka semana ang bata sa oras sa checkup (e.g. "18 weeks").
        /// Dili required — max 50 characters.
        /// </summary>
        [Display(Name = "AOG (Age of Gestation)")]
        [MaxLength(50, ErrorMessage = "Age of gestation must be 50 characters or less.")]
        public string? AOG { get; set; }

        /// <summary>
        /// Timbang sa pasyente sa oras sa checkup (e.g. "58 kg").
        /// Dili required — max 30 characters.
        /// </summary>
        [Display(Name = "WT (Weight)")]
        [MaxLength(30, ErrorMessage = "Weight must be 30 characters or less.")]
        public string? Weight { get; set; }

        /// <summary>
        /// Blood pressure sa pasyente (e.g. "120/80").
        /// Dili required — max 20 characters.
        /// </summary>
        [Display(Name = "BP (Blood Pressure)")]
        [MaxLength(20, ErrorMessage = "Blood pressure must be 20 characters or less.")]
        public string? BloodPressure { get; set; }

        /// <summary>
        /// Temperatura sa lawas sa pasyente sa oras sa checkup (e.g. "36.5").
        /// Dili required — max 20 characters.
        /// </summary>
        [Display(Name = "TEMP (Temperature)")]
        [MaxLength(20, ErrorMessage = "Temperature must be 20 characters or less.")]
        public string? Temperature { get; set; }

        /// <summary>
        /// Fundal Height — sukod gikan sa pubic bone padulong sa ibabaw sa tagoangkan (e.g. "18 cm").
        /// Dili required — max 30 characters.
        /// </summary>
        [Display(Name = "FH (Fundal Height)")]
        [MaxLength(30, ErrorMessage = "Fundal height must be 30 characters or less.")]
        public string? FundalHeight { get; set; }

        /// <summary>
        /// Fetal Heart Tone — tibok sa puso sa bata (e.g. "140 bpm").
        /// Dili required — max 30 characters.
        /// </summary>
        [Display(Name = "FHT (Fetal Heart Tone)")]
        [MaxLength(30, ErrorMessage = "Fetal heart tone must be 30 characters or less.")]
        public string? FetalHeartTone { get; set; }

        /// <summary>
        /// Mga obserbasyon o clinical findings sa checkup.
        /// Dili required — max 500 characters.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Remarks must be 500 characters or less.")]
        public string? Remarks { get; set; }

        /// <summary>
        /// Mga serbisyo nga gi-avail sa pasyente sa kini nga rekord.
        /// Gitipigan isip comma-separated nga mga pangalan sa serbisyo.
        /// Dili required — max 1000 characters.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Selected services list is too long.")]
        [Display(Name = "Services Availed")]
        public string? SelectedServices { get; set; }

        // ==========================================
        // SECTION 1: PRENATAL BASIC INFO
        // ==========================================
        [Display(Name = "Patient Name")]
        [MaxLength(150)]
        public string? PatientName { get; set; }

        [Display(Name = "Patient Address")]
        [MaxLength(250)]
        public string? PatientAddress { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public string? MaritalStatus { get; set; }
        [DataType(DataType.Date)]
        public DateTime? LMP { get; set; }
        [DataType(DataType.Date)]
        public DateTime? EDC { get; set; }
        public string? Menarche { get; set; }
        public string? ContactNo { get; set; }
        public string? Gravida { get; set; }
        public string? TFAL { get; set; }
        public string? Occupation { get; set; }

        // JSON string to hold the dynamic list of visits
        public string? PrenatalVisitsJson { get; set; }

        [NotMapped]
        public List<PrenatalVisit> PrenatalVisits { get; set; } = new List<PrenatalVisit>();

        // ==========================================
        // SECTION 2: ANTENATAL RECORD
        // ==========================================
        public string? FamilyNo { get; set; }
        [DataType(DataType.Date)]
        public DateTime? AntenatalDate { get; set; }
        public string? HusbandName { get; set; }
        public string? VisitType { get; set; } // "initial" or "followup"
        public string? BloodType { get; set; }
        public string? Address { get; set; }
        public string? G { get; set; }
        public string? T { get; set; }
        public string? P { get; set; }
        public string? A { get; set; }
        public string? L { get; set; }

        // ==========================================
        // SECTION 3: ASSESS / CLASSIFY TABLE
        // ==========================================

        // B2 Quick Check
        public string? B2_QuickCheck_Checkboxes { get; set; }
        public string? B2_QuickCheck_Classify { get; set; }

        // B3 RAM
        public string? B3_RAM_Checkboxes { get; set; }
        public string? B3_RAM_Classify { get; set; }

        // Priority Signs
        public string? PrioritySigns_YesNo { get; set; }
        public string? PrioritySigns_Checkboxes { get; set; }
        public string? PrioritySigns_Classify { get; set; }

        // C2 Ask, Check, Record
        public string? C2_MonthsPregnant { get; set; }
        [DataType(DataType.Date)]
        public DateTime? C2_LMP { get; set; }
        [DataType(DataType.Date)]
        public DateTime? C2_EDC { get; set; }
        public string? C2_BabyBefore { get; set; }
        public string? C2_PriorPregnancies { get; set; }
        public string? C2_PriorCaesarian { get; set; }
        public string? C2_PriorTear { get; set; }
        public string? C2_HeavyBleeding { get; set; }
        public string? C2_Convulsions { get; set; }
        public string? C2_Stillbirth { get; set; }
        public string? C2_SmokeDrinkDrugs { get; set; }
        public string? C2_PregnancyWeek { get; set; }
        public string? C2_PlanDeliver { get; set; }
        public string? C2_VaginalBleeding { get; set; }
        public string? C2_BabyMoving { get; set; }
        public string? C2_PreviousComplications { get; set; }
        public string? C2_TT4 { get; set; }
        public string? C2_FHB { get; set; }
        public string? C2_Presentation { get; set; }
        public string? C2_Concerns { get; set; }

        // Third Trimester
        public string? ThirdTrimester_Checkboxes { get; set; }
        public string? ThirdTrimester_Classify { get; set; }

        // C3 Pre-eclampsia
        public string? C3_PreEclampsia_BP { get; set; }
        public string? C3_PreEclampsia_Classify { get; set; }

        // C4 Anemia
        public string? C4_Anemia_Hemoglobin { get; set; }
        public string? C4_Anemia_ConjunctivalPallor { get; set; }
        public string? C4_Anemia_Classify { get; set; }

        // Volunteered Problems
        public string? PatientProblems_YesNo { get; set; }
        public string? PatientProblems_Checkboxes { get; set; }
        public string? PatientProblems_Classify { get; set; }

        // Other Problems / Birth Plan
        public string? AssessOtherProblems { get; set; }
        public string? DevelopBirthPlan { get; set; }
    }

    /// <summary>
    /// Represents a single visit row in the Prenatal Records dynamic table.
    /// </summary>
    public class PrenatalVisit
    {
        public DateTime? RecordDate { get; set; }
        public string? AOG { get; set; }
        public string? Weight { get; set; }
        public string? BloodPressure { get; set; }
        public string? Temperature { get; set; }
        public string? FundalHeight { get; set; }
        public string? FetalHeartTone { get; set; }
        public string? Remarks { get; set; }
    }
}
