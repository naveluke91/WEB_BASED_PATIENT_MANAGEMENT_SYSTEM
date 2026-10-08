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

        // The form fields below are saved by category in JSON columns, not one
        // column each (see STORAGE BY CATEGORY at the end of this class).

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

        // Fundal height, fetal heart tone and remarks are recorded per visit in
        // PrenatalVisitsJson (see PrenatalVisit), so they have no column here.

        /// <summary>
        /// The availed service. It is a copy of Consultation.ServiceType, so it is
        /// no longer stored; the controller still sets it when saving.
        /// </summary>
        [NotMapped]
        [MaxLength(1000, ErrorMessage = "Selected services list is too long.")]
        [Display(Name = "Services Availed")]
        public string? SelectedServices { get; set; }

        // ==========================================
        // SECTION 1: PRENATAL BASIC INFO
        // Patient details are read from the linked Patient (auto-included in
        // ApplicationDbContext) and are not stored again in PrenatalRecords.
        // A value posted by the form is kept in memory for validation only.
        // ==========================================
        private string? _patientName;
        [NotMapped]
        [Display(Name = "Patient Name")]
        [MaxLength(150)]
        public string? PatientName { get => _patientName ?? Patient?.FullName; set => _patientName = value; }

        private DateTime? _dateOfBirth;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get => _dateOfBirth ?? Patient?.DateOfBirth; set => _dateOfBirth = value; }

        private string? _maritalStatus;
        [NotMapped]
        public string? MaritalStatus { get => _maritalStatus ?? Patient?.MaritalStatus; set => _maritalStatus = value; }

        private DateTime? _lmp;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? LMP { get => _lmp ?? Patient?.LMP; set => _lmp = value; }

        private DateTime? _edc;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? EDC { get => _edc ?? Patient?.EDC; set => _edc = value; }

        private string? _menarche;
        [NotMapped]
        public string? Menarche { get => _menarche ?? Patient?.Menarche; set => _menarche = value; }

        private string? _contactNo;
        [NotMapped]
        public string? ContactNo { get => _contactNo ?? Patient?.ContactNo; set => _contactNo = value; }

        private string? _gravida;
        [NotMapped]
        public string? Gravida { get => _gravida ?? Patient?.Gravida; set => _gravida = value; }

        private string? _tfal;
        [NotMapped]
        public string? TFAL { get => _tfal ?? Patient?.TFAL; set => _tfal = value; }

        private string? _occupation;
        [NotMapped]
        public string? Occupation { get => _occupation ?? Patient?.Occupation; set => _occupation = value; }

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

        // Address and G (gravida) are the Patient's, read from Patient (not stored).
        private string? _address;
        [NotMapped]
        public string? Address { get => _address ?? Patient?.Address; set => _address = value; }

        private string? _g;
        [NotMapped]
        public string? G { get => _g ?? Patient?.Gravida; set => _g = value; }

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

        // The same LMP and EDC as the Patient's; read from Patient (not stored).
        private DateTime? _c2Lmp;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? C2_LMP { get => _c2Lmp ?? Patient?.LMP; set => _c2Lmp = value; }

        private DateTime? _c2Edc;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? C2_EDC { get => _c2Edc ?? Patient?.EDC; set => _c2Edc = value; }

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

        // ==========================================
        // STORAGE BY CATEGORY
        // Each category's fields are saved together in one JSON column of
        // PrenatalRecords (mapped in ApplicationDbContext). The fields above stay
        // as they are, so the form, controller and validation use them unchanged.
        // ==========================================

        // General Prenatal Information
        public static readonly string[] GeneralInfoFields =
        {
            nameof(FamilyNo), nameof(AntenatalDate), nameof(VisitType), nameof(HusbandName)
        };

        public string? GeneralInfoJson
        {
            get => CategoryJson.Write(this, GeneralInfoFields);
            set => CategoryJson.Read(this, GeneralInfoFields, value);
        }

        // Vital Signs / Physical Examination
        public static readonly string[] VitalSignsFields =
        {
            nameof(Weight), nameof(BloodPressure), nameof(Temperature)
        };

        public string? VitalSignsJson
        {
            get => CategoryJson.Write(this, VitalSignsFields);
            set => CategoryJson.Read(this, VitalSignsFields, value);
        }

        // Pregnancy / Obstetric Information
        public static readonly string[] ObstetricInfoFields =
        {
            nameof(AOG), nameof(T), nameof(P), nameof(A), nameof(L),
            nameof(C2_MonthsPregnant), nameof(C2_PregnancyWeek), nameof(C2_VaginalBleeding),
            nameof(C2_BabyMoving), nameof(C2_FHB), nameof(C2_Presentation)
        };

        public string? ObstetricInfoJson
        {
            get => CategoryJson.Write(this, ObstetricInfoFields);
            set => CategoryJson.Read(this, ObstetricInfoFields, value);
        }

        // Maternal History
        public static readonly string[] MaternalHistoryFields =
        {
            nameof(C2_BabyBefore), nameof(C2_PriorPregnancies), nameof(C2_PriorCaesarian),
            nameof(C2_PriorTear), nameof(C2_HeavyBleeding), nameof(C2_Convulsions),
            nameof(C2_Stillbirth), nameof(C2_PreviousComplications), nameof(C2_SmokeDrinkDrugs),
            nameof(C2_TT4)
        };

        public string? MaternalHistoryJson
        {
            get => CategoryJson.Write(this, MaternalHistoryFields);
            set => CategoryJson.Read(this, MaternalHistoryFields, value);
        }

        // Laboratory / Diagnostic Information
        public static readonly string[] LaboratoryFields =
        {
            nameof(BloodType), nameof(C4_Anemia_Hemoglobin)
        };

        public string? LaboratoryJson
        {
            get => CategoryJson.Write(this, LaboratoryFields);
            set => CategoryJson.Read(this, LaboratoryFields, value);
        }

        // Pregnancy Risk / Assessment (assess / classify table)
        public static readonly string[] RiskAssessmentFields =
        {
            nameof(B2_QuickCheck_Checkboxes), nameof(B2_QuickCheck_Classify),
            nameof(B3_RAM_Checkboxes), nameof(B3_RAM_Classify),
            nameof(PrioritySigns_YesNo), nameof(PrioritySigns_Checkboxes), nameof(PrioritySigns_Classify),
            nameof(ThirdTrimester_Checkboxes), nameof(ThirdTrimester_Classify),
            nameof(C3_PreEclampsia_BP), nameof(C3_PreEclampsia_Classify),
            nameof(C4_Anemia_ConjunctivalPallor), nameof(C4_Anemia_Classify),
            nameof(PatientProblems_YesNo), nameof(PatientProblems_Checkboxes), nameof(PatientProblems_Classify),
            nameof(AssessOtherProblems), nameof(C2_Concerns)
        };

        public string? RiskAssessmentJson
        {
            get => CategoryJson.Write(this, RiskAssessmentFields);
            set => CategoryJson.Read(this, RiskAssessmentFields, value);
        }

        // Delivery / Follow-up (birth plan)
        public static readonly string[] BirthPlanFields =
        {
            nameof(C2_PlanDeliver), nameof(DevelopBirthPlan)
        };

        public string? BirthPlanJson
        {
            get => CategoryJson.Write(this, BirthPlanFields);
            set => CategoryJson.Read(this, BirthPlanFields, value);
        }
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
