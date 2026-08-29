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

        public string? SelectedService { get; set; } // e.g. "Implant", "DEPO"
        [DataType(DataType.Date)]
        public DateTime? RecordDate { get; set; } = DateTime.Today;

        public string? ClientIdStr { get; set; }

        // Client Name
        public string? ClientLastName { get; set; }
        public string? ClientGivenName { get; set; }
        public string? ClientMI { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ClientDateOfBirth { get; set; }
        public string? ClientAge { get; set; }
        public string? ClientEducation { get; set; }
        public string? ClientOccupation { get; set; }

        // Client Address
        public string? AddressNo { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressBarangay { get; set; }
        public string? AddressMunicipality { get; set; }
        public string? AddressProvince { get; set; }

        public string? ContactNo { get; set; }
        public string? CivilStatus { get; set; }
        public string? Religion { get; set; }

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
        public string? Ack_MethodAccepted { get; set; }
        public string? Ack_ClientPrintedName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Ack_ClientDate { get; set; }
        public string? Ack_ConsentName { get; set; }
        public string? Ack_ParentPrintedName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Ack_ParentDate { get; set; }
    }
}
