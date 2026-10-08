using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    [Table("FamilyPlanningRecords")]
    public class FamilyPlanningRecord
    {
        public int Id { get; set; }

        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        // e.g. "Implant", "DEPO". A copy of Consultation.ServiceType, so it is not
        // stored; the controller still sets it when saving.
        [NotMapped]
        public string? SelectedService { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RecordDate { get; set; } = DateTime.Today;

        // The form fields below are saved by category in JSON columns, not one
        // column each (see STORAGE BY CATEGORY at the end of this class).

        // The client is the linked Patient. Client details that come from Patient
        // are read from it (auto-included in ApplicationDbContext) and are not
        // stored again here. A value posted by the form is kept in memory for
        // validation only.

        // Client Name — split like the consultation form's autofill: the last
        // word is the last name and the remaining words are the given name.
        private string? _clientLastName;
        [NotMapped]
        public string? ClientLastName { get => _clientLastName ?? LastNameOf(Patient?.FullName); set => _clientLastName = value; }

        private string? _clientGivenName;
        [NotMapped]
        public string? ClientGivenName { get => _clientGivenName ?? GivenNameOf(Patient?.FullName); set => _clientGivenName = value; }

        public string? ClientMI { get; set; }

        private DateTime? _clientDateOfBirth;
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime? ClientDateOfBirth { get => _clientDateOfBirth ?? Patient?.DateOfBirth; set => _clientDateOfBirth = value; }

        private string? _clientAge;
        [NotMapped]
        public string? ClientAge { get => _clientAge ?? Patient?.Age.ToString(); set => _clientAge = value; }

        public string? ClientEducation { get; set; }

        private string? _clientOccupation;
        [NotMapped]
        public string? ClientOccupation { get => _clientOccupation ?? Patient?.Occupation; set => _clientOccupation = value; }

        // Client Address — the form fills Street with the Patient's full address.
        public string? AddressNo { get; set; }

        private string? _addressStreet;
        [NotMapped]
        public string? AddressStreet { get => _addressStreet ?? Patient?.Address; set => _addressStreet = value; }

        public string? AddressBarangay { get; set; }
        public string? AddressMunicipality { get; set; }
        public string? AddressProvince { get; set; }

        private string? _contactNo;
        [NotMapped]
        public string? ContactNo { get => _contactNo ?? Patient?.ContactNo; set => _contactNo = value; }

        private string? _civilStatus;
        [NotMapped]
        public string? CivilStatus { get => _civilStatus ?? Patient?.MaritalStatus; set => _civilStatus = value; }

        private string? _religion;
        [NotMapped]
        public string? Religion { get => _religion ?? Patient?.Religion; set => _religion = value; }

        // Spouse Name
        public string? SpouseLastName { get; set; }
        public string? SpouseGivenName { get; set; }
        public string? SpouseMI { get; set; }
        [DataType(DataType.Date)]
        public DateTime? SpouseDateOfBirth { get; set; }
        public string? SpouseAge { get; set; }
        public string? SpouseOccupation { get; set; }

        // Children & Income
        public string? NoOfLivingChildren { get; set; }
        public string? PlanMoreChildren { get; set; }
        public string? AverageMonthlyIncome { get; set; }

        // Type of Client
        public string? TypeOfClient { get; set; }
        public string? ReasonForFP { get; set; } // stored as comma separated
        public string? ReasonForFP_Others { get; set; }
        public string? ReasonChanging { get; set; } // stored as comma separated
        public string? MethodCurrentlyUsed { get; set; } // stored as comma separated
        public string? MethodCurrentlyUsed_Others { get; set; }

        // I. Medical History (Yes/No)
        public string? MH_SevereHeadaches { get; set; }
        public string? MH_StrokeHeartHypertension { get; set; }
        public string? MH_HematomaBruising { get; set; }
        public string? MH_BreastCancerMass { get; set; }
        public string? MH_SevereChestPain { get; set; }
        public string? MH_Cough14Days { get; set; }
        public string? MH_Jaundice { get; set; }
        public string? MH_UnexplainedVaginalBleeding { get; set; }
        public string? MH_AbnormalVaginalDischarge { get; set; }
        public string? MH_PhenobarbitalRifampicin { get; set; }
        public string? MH_Smoker { get; set; }
        public string? MH_WithDisability { get; set; }
        public string? MH_DisabilitySpecify { get; set; }

        // II. Obstetrical History
        public string? OH_Gravida { get; set; }
        public string? OH_Para { get; set; }
        public string? OH_FullTerm { get; set; }
        public string? OH_Premature { get; set; }
        public string? OH_Abortion { get; set; }
        public string? OH_LivingChildren { get; set; }
        [DataType(DataType.Date)]
        public DateTime? OH_DateOfLastDelivery { get; set; }
        public string? OH_TypeOfLastDelivery { get; set; } // vaginal / cs
        [DataType(DataType.Date)]
        public DateTime? OH_LastMenstrualPeriod { get; set; }
        [DataType(DataType.Date)]
        public DateTime? OH_PreviousMenstrualPeriod { get; set; }
        public string? OH_MenstrualFlow { get; set; }
        public string? OH_Dysmenorrhea { get; set; }
        public string? OH_HydatidiformMole { get; set; }
        public string? OH_EctopicPregnancy { get; set; }

        // III. STI Risks
        public string? STI_AbnormalDischarge { get; set; }
        public string? STI_AbnormalDischarge_Loc { get; set; }
        public string? STI_SoresUlcers { get; set; }
        public string? STI_PainBurning { get; set; }
        public string? STI_HistoryTreatment { get; set; }
        public string? STI_HIV_PID { get; set; }

        // IV. VAW Risks
        public string? VAW_UnpleasantRelationship { get; set; }
        public string? VAW_PartnerDisapprove { get; set; }
        public string? VAW_HistoryDomesticViolence { get; set; }
        public string? VAW_ReferredTo { get; set; } // comma separated
        public string? VAW_ReferredTo_Others { get; set; }

        // V. Physical Examination
        public string? PE_Height { get; set; }
        public string? PE_Weight { get; set; }
        public string? PE_BloodPressure_Systolic { get; set; }
        public string? PE_BloodPressure_Diastolic { get; set; }
        public string? PE_PulseRate { get; set; }
        
        public string? PE_Skin { get; set; } // comma separated
        public string? PE_Conjunctiva { get; set; }
        public string? PE_Neck { get; set; }
        public string? PE_Breast { get; set; }
        public string? PE_Abdomen { get; set; }
        public string? PE_Extremities { get; set; }

        // Pelvic Examination
        public string? Pelvic_Normal { get; set; }
        public string? Pelvic_Mass { get; set; }
        public string? Pelvic_AbnormalDischarge { get; set; }
        public string? Pelvic_CervicalAbnormalities { get; set; } // comma separated
        public string? Pelvic_CervicalConsistency { get; set; }
        public string? Pelvic_CervicalTenderness { get; set; }
        public string? Pelvic_AdnexalMassTenderness { get; set; }
        public string? Pelvic_UterinePosition { get; set; }
        public string? Pelvic_UterineDepth { get; set; }

        // Acknowledgement
        // The accepted method is the consultation's service (Consultation.ServiceType);
        // the controller sets it on load and save, so it is not stored.
        [NotMapped]
        public string? Ack_MethodAccepted { get; set; }
        public string? Ack_ClientPrintedName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Ack_ClientDate { get; set; }
        public string? Ack_ConsentName { get; set; }
        public string? Ack_ParentPrintedName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Ack_ParentDate { get; set; }

        // ==========================================
        // STORAGE BY CATEGORY
        // Each section of FP Form 1 is saved together in one JSON column of
        // FamilyPlanningRecords (mapped in ApplicationDbContext). The fields above
        // stay as they are, so the form, controller and validation use them unchanged.
        // ==========================================

        // Client Information (client, address, spouse, children and income)
        public static readonly string[] ClientInfoFields =
        {
            nameof(ClientMI), nameof(ClientEducation),
            nameof(AddressNo), nameof(AddressBarangay), nameof(AddressMunicipality), nameof(AddressProvince),
            nameof(SpouseLastName), nameof(SpouseGivenName), nameof(SpouseMI),
            nameof(SpouseDateOfBirth), nameof(SpouseAge), nameof(SpouseOccupation),
            nameof(NoOfLivingChildren), nameof(PlanMoreChildren), nameof(AverageMonthlyIncome)
        };

        public string? ClientInfoJson
        {
            get => CategoryJson.Write(this, ClientInfoFields);
            set => CategoryJson.Read(this, ClientInfoFields, value);
        }

        // Family Planning Method / History (type of client, reasons, current method)
        public static readonly string[] FamilyPlanningMethodFields =
        {
            nameof(TypeOfClient), nameof(ReasonForFP), nameof(ReasonForFP_Others),
            nameof(ReasonChanging), nameof(MethodCurrentlyUsed), nameof(MethodCurrentlyUsed_Others)
        };

        public string? FamilyPlanningMethodJson
        {
            get => CategoryJson.Write(this, FamilyPlanningMethodFields);
            set => CategoryJson.Read(this, FamilyPlanningMethodFields, value);
        }

        // I. Medical History
        public static readonly string[] MedicalHistoryFields =
        {
            nameof(MH_SevereHeadaches), nameof(MH_StrokeHeartHypertension), nameof(MH_HematomaBruising),
            nameof(MH_BreastCancerMass), nameof(MH_SevereChestPain), nameof(MH_Cough14Days),
            nameof(MH_Jaundice), nameof(MH_UnexplainedVaginalBleeding), nameof(MH_AbnormalVaginalDischarge),
            nameof(MH_PhenobarbitalRifampicin), nameof(MH_Smoker), nameof(MH_WithDisability),
            nameof(MH_DisabilitySpecify)
        };

        public string? MedicalHistoryJson
        {
            get => CategoryJson.Write(this, MedicalHistoryFields);
            set => CategoryJson.Read(this, MedicalHistoryFields, value);
        }

        // II. Obstetrical History
        public static readonly string[] ObstetricalHistoryFields =
        {
            nameof(OH_Gravida), nameof(OH_Para), nameof(OH_FullTerm), nameof(OH_Premature),
            nameof(OH_Abortion), nameof(OH_LivingChildren), nameof(OH_DateOfLastDelivery),
            nameof(OH_TypeOfLastDelivery), nameof(OH_LastMenstrualPeriod), nameof(OH_PreviousMenstrualPeriod),
            nameof(OH_MenstrualFlow), nameof(OH_Dysmenorrhea), nameof(OH_HydatidiformMole),
            nameof(OH_EctopicPregnancy)
        };

        public string? ObstetricalHistoryJson
        {
            get => CategoryJson.Write(this, ObstetricalHistoryFields);
            set => CategoryJson.Read(this, ObstetricalHistoryFields, value);
        }

        // III. Risks for STI and IV. Risks for VAW
        public static readonly string[] RiskAssessmentFields =
        {
            nameof(STI_AbnormalDischarge), nameof(STI_AbnormalDischarge_Loc), nameof(STI_SoresUlcers),
            nameof(STI_PainBurning), nameof(STI_HistoryTreatment), nameof(STI_HIV_PID),
            nameof(VAW_UnpleasantRelationship), nameof(VAW_PartnerDisapprove),
            nameof(VAW_HistoryDomesticViolence), nameof(VAW_ReferredTo), nameof(VAW_ReferredTo_Others)
        };

        public string? RiskAssessmentJson
        {
            get => CategoryJson.Write(this, RiskAssessmentFields);
            set => CategoryJson.Read(this, RiskAssessmentFields, value);
        }

        // V. Physical Examination (including the pelvic examination)
        public static readonly string[] PhysicalExamFields =
        {
            nameof(PE_Height), nameof(PE_Weight), nameof(PE_BloodPressure_Systolic),
            nameof(PE_BloodPressure_Diastolic), nameof(PE_PulseRate), nameof(PE_Skin),
            nameof(PE_Conjunctiva), nameof(PE_Neck), nameof(PE_Breast), nameof(PE_Abdomen),
            nameof(PE_Extremities),
            nameof(Pelvic_Normal), nameof(Pelvic_Mass), nameof(Pelvic_AbnormalDischarge),
            nameof(Pelvic_CervicalAbnormalities), nameof(Pelvic_CervicalConsistency),
            nameof(Pelvic_CervicalTenderness), nameof(Pelvic_AdnexalMassTenderness),
            nameof(Pelvic_UterinePosition), nameof(Pelvic_UterineDepth)
        };

        public string? PhysicalExamJson
        {
            get => CategoryJson.Write(this, PhysicalExamFields);
            set => CategoryJson.Read(this, PhysicalExamFields, value);
        }

        // Acknowledgement / Consent
        public static readonly string[] AcknowledgementFields =
        {
            nameof(Ack_ClientPrintedName), nameof(Ack_ClientDate), nameof(Ack_ConsentName),
            nameof(Ack_ParentPrintedName), nameof(Ack_ParentDate)
        };

        public string? AcknowledgementJson
        {
            get => CategoryJson.Write(this, AcknowledgementFields);
            set => CategoryJson.Read(this, AcknowledgementFields, value);
        }

        private static string[] NameParts(string? fullName) =>
            (fullName ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        private static string? LastNameOf(string? fullName)
        {
            var parts = NameParts(fullName);
            return parts.Length == 0 ? null : parts[^1];
        }

        private static string? GivenNameOf(string? fullName)
        {
            var parts = NameParts(fullName);
            return parts.Length switch
            {
                0 => null,
                1 => parts[0],
                _ => string.Join(' ', parts[..^1])
            };
        }
    }
}
