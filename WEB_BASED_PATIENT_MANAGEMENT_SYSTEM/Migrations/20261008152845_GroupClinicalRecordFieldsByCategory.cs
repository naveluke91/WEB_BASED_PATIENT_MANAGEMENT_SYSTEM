using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <summary>
    /// Shortens PrenatalRecords (54 -> 11 columns) and FamilyPlanningRecords
    /// (87 -> 10 columns): each form category is stored in one JSON column instead
    /// of one column per input. Existing values are copied into the JSON columns
    /// before the old columns are dropped. No table is added or removed.
    /// </summary>
    public partial class GroupClinicalRecordFieldsByCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "PrenatalVisitsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Prenatal visit rows (JSON list). Fields per visit: RecordDate, AOG, Weight, BloodPressure, Temperature, FundalHeight, FetalHeartTone, Remarks",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "BirthPlanJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Delivery / Follow-up (JSON). Fields: C2_PlanDeliver, DevelopBirthPlan");

            migrationBuilder.AddColumn<string>(
                name: "GeneralInfoJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information (JSON). Fields: FamilyNo, AntenatalDate, VisitType, HusbandName");

            migrationBuilder.AddColumn<string>(
                name: "LaboratoryJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Laboratory / Diagnostic Information (JSON). Fields: BloodType, C4_Anemia_Hemoglobin");

            migrationBuilder.AddColumn<string>(
                name: "MaternalHistoryJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History (JSON). Fields: C2_BabyBefore, C2_PriorPregnancies, C2_PriorCaesarian, C2_PriorTear, C2_HeavyBleeding, C2_Convulsions, C2_Stillbirth, C2_PreviousComplications, C2_SmokeDrinkDrugs, C2_TT4");

            migrationBuilder.AddColumn<string>(
                name: "ObstetricInfoJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information (JSON). Fields: AOG, T, P, A, L, C2_MonthsPregnant, C2_PregnancyWeek, C2_VaginalBleeding, C2_BabyMoving, C2_FHB, C2_Presentation");

            migrationBuilder.AddColumn<string>(
                name: "RiskAssessmentJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment (JSON). Fields: B2_QuickCheck_Checkboxes, B2_QuickCheck_Classify, B3_RAM_Checkboxes, B3_RAM_Classify, PrioritySigns_YesNo, PrioritySigns_Checkboxes, PrioritySigns_Classify, ThirdTrimester_Checkboxes, ThirdTrimester_Classify, C3_PreEclampsia_BP, C3_PreEclampsia_Classify, C4_Anemia_ConjunctivalPallor, C4_Anemia_Classify, PatientProblems_YesNo, PatientProblems_Checkboxes, PatientProblems_Classify, AssessOtherProblems, C2_Concerns");

            migrationBuilder.AddColumn<string>(
                name: "VitalSignsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination (JSON). Fields: Weight, BloodPressure, Temperature");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgementJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent (JSON). Fields: Ack_ClientPrintedName, Ack_ClientDate, Ack_ConsentName, Ack_ParentPrintedName, Ack_ParentDate");

            migrationBuilder.AddColumn<string>(
                name: "ClientInfoJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information (JSON). Fields: ClientMI, ClientEducation, AddressNo, AddressBarangay, AddressMunicipality, AddressProvince, SpouseLastName, SpouseGivenName, SpouseMI, SpouseDateOfBirth, SpouseAge, SpouseOccupation, NoOfLivingChildren, PlanMoreChildren, AverageMonthlyIncome");

            migrationBuilder.AddColumn<string>(
                name: "FamilyPlanningMethodJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History (JSON). Fields: TypeOfClient, ReasonForFP, ReasonForFP_Others, ReasonChanging, MethodCurrentlyUsed, MethodCurrentlyUsed_Others");

            migrationBuilder.AddColumn<string>(
                name: "MedicalHistoryJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: I. Medical History (JSON). Fields: MH_SevereHeadaches, MH_StrokeHeartHypertension, MH_HematomaBruising, MH_BreastCancerMass, MH_SevereChestPain, MH_Cough14Days, MH_Jaundice, MH_UnexplainedVaginalBleeding, MH_AbnormalVaginalDischarge, MH_PhenobarbitalRifampicin, MH_Smoker, MH_WithDisability, MH_DisabilitySpecify");

            migrationBuilder.AddColumn<string>(
                name: "ObstetricalHistoryJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: II. Obstetrical History (JSON). Fields: OH_Gravida, OH_Para, OH_FullTerm, OH_Premature, OH_Abortion, OH_LivingChildren, OH_DateOfLastDelivery, OH_TypeOfLastDelivery, OH_LastMenstrualPeriod, OH_PreviousMenstrualPeriod, OH_MenstrualFlow, OH_Dysmenorrhea, OH_HydatidiformMole, OH_EctopicPregnancy");

            migrationBuilder.AddColumn<string>(
                name: "PhysicalExamJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: V. Physical Examination (JSON). Fields: PE_Height, PE_Weight, PE_BloodPressure_Systolic, PE_BloodPressure_Diastolic, PE_PulseRate, PE_Skin, PE_Conjunctiva, PE_Neck, PE_Breast, PE_Abdomen, PE_Extremities, Pelvic_Normal, Pelvic_Mass, Pelvic_AbnormalDischarge, Pelvic_CervicalAbnormalities, Pelvic_CervicalConsistency, Pelvic_CervicalTenderness, Pelvic_AdnexalMassTenderness, Pelvic_UterinePosition, Pelvic_UterineDepth");

            migrationBuilder.AddColumn<string>(
                name: "RiskAssessmentJson",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: III-IV. Risks for STI and VAW (JSON). Fields: STI_AbnormalDischarge, STI_AbnormalDischarge_Loc, STI_SoresUlcers, STI_PainBurning, STI_HistoryTreatment, STI_HIV_PID, VAW_UnpleasantRelationship, VAW_PartnerDisapprove, VAW_HistoryDomesticViolence, VAW_ReferredTo, VAW_ReferredTo_Others");

            // Copy each category's values into its JSON column before the old
            // one-column-per-input columns are dropped, so no data is lost.
            migrationBuilder.Sql(@"
UPDATE r SET
    r.[GeneralInfoJson] = NULLIF((SELECT r.[FamilyNo], r.[AntenatalDate], r.[VisitType], r.[HusbandName] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[VitalSignsJson] = NULLIF((SELECT r.[Weight], r.[BloodPressure], r.[Temperature] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[ObstetricInfoJson] = NULLIF((SELECT r.[AOG], r.[T], r.[P], r.[A], r.[L], r.[C2_MonthsPregnant], r.[C2_PregnancyWeek], r.[C2_VaginalBleeding], r.[C2_BabyMoving], r.[C2_FHB], r.[C2_Presentation] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[MaternalHistoryJson] = NULLIF((SELECT r.[C2_BabyBefore], r.[C2_PriorPregnancies], r.[C2_PriorCaesarian], r.[C2_PriorTear], r.[C2_HeavyBleeding], r.[C2_Convulsions], r.[C2_Stillbirth], r.[C2_PreviousComplications], r.[C2_SmokeDrinkDrugs], r.[C2_TT4] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[LaboratoryJson] = NULLIF((SELECT r.[BloodType], r.[C4_Anemia_Hemoglobin] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[RiskAssessmentJson] = NULLIF((SELECT r.[B2_QuickCheck_Checkboxes], r.[B2_QuickCheck_Classify], r.[B3_RAM_Checkboxes], r.[B3_RAM_Classify], r.[PrioritySigns_YesNo], r.[PrioritySigns_Checkboxes], r.[PrioritySigns_Classify], r.[ThirdTrimester_Checkboxes], r.[ThirdTrimester_Classify], r.[C3_PreEclampsia_BP], r.[C3_PreEclampsia_Classify], r.[C4_Anemia_ConjunctivalPallor], r.[C4_Anemia_Classify], r.[PatientProblems_YesNo], r.[PatientProblems_Checkboxes], r.[PatientProblems_Classify], r.[AssessOtherProblems], r.[C2_Concerns] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[BirthPlanJson] = NULLIF((SELECT r.[C2_PlanDeliver], r.[DevelopBirthPlan] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}')
FROM [PrenatalRecords] r;

UPDATE r SET
    r.[ClientInfoJson] = NULLIF((SELECT r.[ClientMI], r.[ClientEducation], r.[AddressNo], r.[AddressBarangay], r.[AddressMunicipality], r.[AddressProvince], r.[SpouseLastName], r.[SpouseGivenName], r.[SpouseMI], r.[SpouseDateOfBirth], r.[SpouseAge], r.[SpouseOccupation], r.[NoOfLivingChildren], r.[PlanMoreChildren], r.[AverageMonthlyIncome] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[FamilyPlanningMethodJson] = NULLIF((SELECT r.[TypeOfClient], r.[ReasonForFP], r.[ReasonForFP_Others], r.[ReasonChanging], r.[MethodCurrentlyUsed], r.[MethodCurrentlyUsed_Others] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[MedicalHistoryJson] = NULLIF((SELECT r.[MH_SevereHeadaches], r.[MH_StrokeHeartHypertension], r.[MH_HematomaBruising], r.[MH_BreastCancerMass], r.[MH_SevereChestPain], r.[MH_Cough14Days], r.[MH_Jaundice], r.[MH_UnexplainedVaginalBleeding], r.[MH_AbnormalVaginalDischarge], r.[MH_PhenobarbitalRifampicin], r.[MH_Smoker], r.[MH_WithDisability], r.[MH_DisabilitySpecify] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[ObstetricalHistoryJson] = NULLIF((SELECT r.[OH_Gravida], r.[OH_Para], r.[OH_FullTerm], r.[OH_Premature], r.[OH_Abortion], r.[OH_LivingChildren], r.[OH_DateOfLastDelivery], r.[OH_TypeOfLastDelivery], r.[OH_LastMenstrualPeriod], r.[OH_PreviousMenstrualPeriod], r.[OH_MenstrualFlow], r.[OH_Dysmenorrhea], r.[OH_HydatidiformMole], r.[OH_EctopicPregnancy] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[RiskAssessmentJson] = NULLIF((SELECT r.[STI_AbnormalDischarge], r.[STI_AbnormalDischarge_Loc], r.[STI_SoresUlcers], r.[STI_PainBurning], r.[STI_HistoryTreatment], r.[STI_HIV_PID], r.[VAW_UnpleasantRelationship], r.[VAW_PartnerDisapprove], r.[VAW_HistoryDomesticViolence], r.[VAW_ReferredTo], r.[VAW_ReferredTo_Others] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[PhysicalExamJson] = NULLIF((SELECT r.[PE_Height], r.[PE_Weight], r.[PE_BloodPressure_Systolic], r.[PE_BloodPressure_Diastolic], r.[PE_PulseRate], r.[PE_Skin], r.[PE_Conjunctiva], r.[PE_Neck], r.[PE_Breast], r.[PE_Abdomen], r.[PE_Extremities], r.[Pelvic_Normal], r.[Pelvic_Mass], r.[Pelvic_AbnormalDischarge], r.[Pelvic_CervicalAbnormalities], r.[Pelvic_CervicalConsistency], r.[Pelvic_CervicalTenderness], r.[Pelvic_AdnexalMassTenderness], r.[Pelvic_UterinePosition], r.[Pelvic_UterineDepth] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}'),
    r.[AcknowledgementJson] = NULLIF((SELECT r.[Ack_ClientPrintedName], r.[Ack_ClientDate], r.[Ack_ConsentName], r.[Ack_ParentPrintedName], r.[Ack_ParentDate] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'{}')
FROM [FamilyPlanningRecords] r;
");

            migrationBuilder.DropColumn(
                name: "A",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "AOG",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "AntenatalDate",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "AssessOtherProblems",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "B2_QuickCheck_Checkboxes",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "B2_QuickCheck_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "B3_RAM_Checkboxes",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "B3_RAM_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "BloodPressure",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_BabyBefore",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_BabyMoving",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_Concerns",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_Convulsions",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_FHB",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_MonthsPregnant",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PlanDeliver",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PregnancyWeek",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_Presentation",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PreviousComplications",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PriorCaesarian",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PriorPregnancies",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_PriorTear",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_SmokeDrinkDrugs",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_Stillbirth",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_TT4",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_VaginalBleeding",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C3_PreEclampsia_BP",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C3_PreEclampsia_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C4_Anemia_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C4_Anemia_ConjunctivalPallor",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C4_Anemia_Hemoglobin",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "FamilyNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "HusbandName",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "L",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "P",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientProblems_Checkboxes",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientProblems_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientProblems_YesNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PrioritySigns_Checkboxes",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PrioritySigns_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PrioritySigns_YesNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "T",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "ThirdTrimester_Checkboxes",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "ThirdTrimester_Classify",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "VisitType",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Ack_ClientDate",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Ack_ClientPrintedName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Ack_ConsentName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Ack_ParentDate",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Ack_ParentPrintedName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AddressBarangay",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AddressMunicipality",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AddressNo",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AddressProvince",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AverageMonthlyIncome",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientEducation",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientMI",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_AbnormalVaginalDischarge",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_BreastCancerMass",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_Cough14Days",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_DisabilitySpecify",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_HematomaBruising",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_Jaundice",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_PhenobarbitalRifampicin",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_SevereChestPain",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_SevereHeadaches",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_Smoker",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_StrokeHeartHypertension",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_UnexplainedVaginalBleeding",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MH_WithDisability",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MethodCurrentlyUsed",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MethodCurrentlyUsed_Others",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "NoOfLivingChildren",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_Abortion",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_DateOfLastDelivery",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_Dysmenorrhea",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_EctopicPregnancy",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_FullTerm",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_Gravida",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_HydatidiformMole",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_LastMenstrualPeriod",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_LivingChildren",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_MenstrualFlow",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_Para",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_Premature",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_PreviousMenstrualPeriod",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "OH_TypeOfLastDelivery",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Abdomen",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_BloodPressure_Diastolic",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_BloodPressure_Systolic",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Breast",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Conjunctiva",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Extremities",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Height",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Neck",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_PulseRate",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Skin",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PE_Weight",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_AbnormalDischarge",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_AdnexalMassTenderness",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_CervicalAbnormalities",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_CervicalConsistency",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_CervicalTenderness",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_Mass",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_Normal",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_UterineDepth",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Pelvic_UterinePosition",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PlanMoreChildren",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ReasonChanging",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ReasonForFP",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ReasonForFP_Others",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_AbnormalDischarge",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_AbnormalDischarge_Loc",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_HIV_PID",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_HistoryTreatment",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_PainBurning",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "STI_SoresUlcers",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseAge",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseDateOfBirth",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseGivenName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseLastName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseMI",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SpouseOccupation",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "TypeOfClient",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "VAW_HistoryDomesticViolence",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "VAW_PartnerDisapprove",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "VAW_ReferredTo",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "VAW_ReferredTo_Others",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "VAW_UnpleasantRelationship",
                table: "FamilyPlanningRecords");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "date",
                nullable: false,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "PrenatalVisitsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Prenatal visit rows (JSON list). Fields per visit: RecordDate, AOG, Weight, BloodPressure, Temperature, FundalHeight, FetalHeartTone, Remarks");

            migrationBuilder.AddColumn<string>(
                name: "A",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "AOG",
                table: "PrenatalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<DateTime>(
                name: "AntenatalDate",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: General Prenatal Information");

            migrationBuilder.AddColumn<string>(
                name: "AssessOtherProblems",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "B2_QuickCheck_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "B2_QuickCheck_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "B3_RAM_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "B3_RAM_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "BloodPressure",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Laboratory / Diagnostic Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_BabyBefore",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_BabyMoving",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_Concerns",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "C2_Convulsions",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_FHB",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_MonthsPregnant",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_PlanDeliver",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Delivery / Follow-up");

            migrationBuilder.AddColumn<string>(
                name: "C2_PregnancyWeek",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_Presentation",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C2_PreviousComplications",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorCaesarian",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorPregnancies",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorTear",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_SmokeDrinkDrugs",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_Stillbirth",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_TT4",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History");

            migrationBuilder.AddColumn<string>(
                name: "C2_VaginalBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "C3_PreEclampsia_BP",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "C3_PreEclampsia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_ConjunctivalPallor",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_Hemoglobin",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Laboratory / Diagnostic Information");

            migrationBuilder.AddColumn<string>(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Delivery / Follow-up");

            migrationBuilder.AddColumn<string>(
                name: "FamilyNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information");

            migrationBuilder.AddColumn<string>(
                name: "HusbandName",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information");

            migrationBuilder.AddColumn<string>(
                name: "L",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "P",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "T",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "Temperature",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "ThirdTrimester_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "ThirdTrimester_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AddColumn<string>(
                name: "VisitType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information");

            migrationBuilder.AddColumn<string>(
                name: "Weight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Ack_ClientDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<string>(
                name: "Ack_ClientPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<string>(
                name: "Ack_ConsentName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<DateTime>(
                name: "Ack_ParentDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<string>(
                name: "Ack_ParentPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<string>(
                name: "AddressBarangay",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "AddressMunicipality",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "AddressNo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "AddressProvince",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "AverageMonthlyIncome",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "ClientEducation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "ClientMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "MH_AbnormalVaginalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_BreastCancerMass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_Cough14Days",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_DisabilitySpecify",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_HematomaBruising",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_Jaundice",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_PhenobarbitalRifampicin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_SevereChestPain",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_SevereHeadaches",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_Smoker",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_StrokeHeartHypertension",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_UnexplainedVaginalBleeding",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MH_WithDisability",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment");

            migrationBuilder.AddColumn<string>(
                name: "MethodCurrentlyUsed",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "MethodCurrentlyUsed_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "NoOfLivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_Abortion",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<DateTime>(
                name: "OH_DateOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_Dysmenorrhea",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_EctopicPregnancy",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_FullTerm",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_Gravida",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_HydatidiformMole",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<DateTime>(
                name: "OH_LastMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_LivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_MenstrualFlow",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_Para",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_Premature",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<DateTime>(
                name: "OH_PreviousMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "OH_TypeOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "PE_Abdomen",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_BloodPressure_Diastolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_BloodPressure_Systolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Breast",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Conjunctiva",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Extremities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Height",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Neck",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_PulseRate",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Skin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PE_Weight",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_AdnexalMassTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_CervicalAbnormalities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_CervicalConsistency",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_CervicalTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_Mass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_Normal",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_UterineDepth",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "Pelvic_UterinePosition",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination");

            migrationBuilder.AddColumn<string>(
                name: "PlanMoreChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "ReasonChanging",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForFP",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForFP_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "STI_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "STI_AbnormalDischarge_Loc",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "STI_HIV_PID",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "STI_HistoryTreatment",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "STI_PainBurning",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "STI_SoresUlcers",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "SpouseAge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<DateTime>(
                name: "SpouseDateOfBirth",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "SpouseGivenName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "SpouseLastName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "SpouseMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "SpouseOccupation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information");

            migrationBuilder.AddColumn<string>(
                name: "TypeOfClient",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History");

            migrationBuilder.AddColumn<string>(
                name: "VAW_HistoryDomesticViolence",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "VAW_PartnerDisapprove",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "VAW_ReferredTo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "VAW_ReferredTo_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AddColumn<string>(
                name: "VAW_UnpleasantRelationship",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)");

            // Copy the values back out of the JSON columns into the restored columns.
            migrationBuilder.Sql(@"
UPDATE r SET
    r.[FamilyNo] = j1.[FamilyNo],
    r.[AntenatalDate] = j1.[AntenatalDate],
    r.[VisitType] = j1.[VisitType],
    r.[HusbandName] = j1.[HusbandName],
    r.[Weight] = j2.[Weight],
    r.[BloodPressure] = j2.[BloodPressure],
    r.[Temperature] = j2.[Temperature],
    r.[AOG] = j3.[AOG],
    r.[T] = j3.[T],
    r.[P] = j3.[P],
    r.[A] = j3.[A],
    r.[L] = j3.[L],
    r.[C2_MonthsPregnant] = j3.[C2_MonthsPregnant],
    r.[C2_PregnancyWeek] = j3.[C2_PregnancyWeek],
    r.[C2_VaginalBleeding] = j3.[C2_VaginalBleeding],
    r.[C2_BabyMoving] = j3.[C2_BabyMoving],
    r.[C2_FHB] = j3.[C2_FHB],
    r.[C2_Presentation] = j3.[C2_Presentation],
    r.[C2_BabyBefore] = j4.[C2_BabyBefore],
    r.[C2_PriorPregnancies] = j4.[C2_PriorPregnancies],
    r.[C2_PriorCaesarian] = j4.[C2_PriorCaesarian],
    r.[C2_PriorTear] = j4.[C2_PriorTear],
    r.[C2_HeavyBleeding] = j4.[C2_HeavyBleeding],
    r.[C2_Convulsions] = j4.[C2_Convulsions],
    r.[C2_Stillbirth] = j4.[C2_Stillbirth],
    r.[C2_PreviousComplications] = j4.[C2_PreviousComplications],
    r.[C2_SmokeDrinkDrugs] = j4.[C2_SmokeDrinkDrugs],
    r.[C2_TT4] = j4.[C2_TT4],
    r.[BloodType] = j5.[BloodType],
    r.[C4_Anemia_Hemoglobin] = j5.[C4_Anemia_Hemoglobin],
    r.[B2_QuickCheck_Checkboxes] = j6.[B2_QuickCheck_Checkboxes],
    r.[B2_QuickCheck_Classify] = j6.[B2_QuickCheck_Classify],
    r.[B3_RAM_Checkboxes] = j6.[B3_RAM_Checkboxes],
    r.[B3_RAM_Classify] = j6.[B3_RAM_Classify],
    r.[PrioritySigns_YesNo] = j6.[PrioritySigns_YesNo],
    r.[PrioritySigns_Checkboxes] = j6.[PrioritySigns_Checkboxes],
    r.[PrioritySigns_Classify] = j6.[PrioritySigns_Classify],
    r.[ThirdTrimester_Checkboxes] = j6.[ThirdTrimester_Checkboxes],
    r.[ThirdTrimester_Classify] = j6.[ThirdTrimester_Classify],
    r.[C3_PreEclampsia_BP] = j6.[C3_PreEclampsia_BP],
    r.[C3_PreEclampsia_Classify] = j6.[C3_PreEclampsia_Classify],
    r.[C4_Anemia_ConjunctivalPallor] = j6.[C4_Anemia_ConjunctivalPallor],
    r.[C4_Anemia_Classify] = j6.[C4_Anemia_Classify],
    r.[PatientProblems_YesNo] = j6.[PatientProblems_YesNo],
    r.[PatientProblems_Checkboxes] = j6.[PatientProblems_Checkboxes],
    r.[PatientProblems_Classify] = j6.[PatientProblems_Classify],
    r.[AssessOtherProblems] = j6.[AssessOtherProblems],
    r.[C2_Concerns] = j6.[C2_Concerns],
    r.[C2_PlanDeliver] = j7.[C2_PlanDeliver],
    r.[DevelopBirthPlan] = j7.[DevelopBirthPlan]
FROM [PrenatalRecords] r
OUTER APPLY OPENJSON(r.[GeneralInfoJson]) WITH ([FamilyNo] nvarchar(max), [AntenatalDate] datetime2, [VisitType] nvarchar(max), [HusbandName] nvarchar(max)) j1
OUTER APPLY OPENJSON(r.[VitalSignsJson]) WITH ([Weight] nvarchar(30), [BloodPressure] nvarchar(20), [Temperature] nvarchar(20)) j2
OUTER APPLY OPENJSON(r.[ObstetricInfoJson]) WITH ([AOG] nvarchar(50), [T] nvarchar(max), [P] nvarchar(max), [A] nvarchar(max), [L] nvarchar(max), [C2_MonthsPregnant] nvarchar(max), [C2_PregnancyWeek] nvarchar(max), [C2_VaginalBleeding] nvarchar(max), [C2_BabyMoving] nvarchar(max), [C2_FHB] nvarchar(max), [C2_Presentation] nvarchar(max)) j3
OUTER APPLY OPENJSON(r.[MaternalHistoryJson]) WITH ([C2_BabyBefore] nvarchar(max), [C2_PriorPregnancies] nvarchar(max), [C2_PriorCaesarian] nvarchar(max), [C2_PriorTear] nvarchar(max), [C2_HeavyBleeding] nvarchar(max), [C2_Convulsions] nvarchar(max), [C2_Stillbirth] nvarchar(max), [C2_PreviousComplications] nvarchar(max), [C2_SmokeDrinkDrugs] nvarchar(max), [C2_TT4] nvarchar(max)) j4
OUTER APPLY OPENJSON(r.[LaboratoryJson]) WITH ([BloodType] nvarchar(max), [C4_Anemia_Hemoglobin] nvarchar(max)) j5
OUTER APPLY OPENJSON(r.[RiskAssessmentJson]) WITH ([B2_QuickCheck_Checkboxes] nvarchar(max), [B2_QuickCheck_Classify] nvarchar(max), [B3_RAM_Checkboxes] nvarchar(max), [B3_RAM_Classify] nvarchar(max), [PrioritySigns_YesNo] nvarchar(max), [PrioritySigns_Checkboxes] nvarchar(max), [PrioritySigns_Classify] nvarchar(max), [ThirdTrimester_Checkboxes] nvarchar(max), [ThirdTrimester_Classify] nvarchar(max), [C3_PreEclampsia_BP] nvarchar(max), [C3_PreEclampsia_Classify] nvarchar(max), [C4_Anemia_ConjunctivalPallor] nvarchar(max), [C4_Anemia_Classify] nvarchar(max), [PatientProblems_YesNo] nvarchar(max), [PatientProblems_Checkboxes] nvarchar(max), [PatientProblems_Classify] nvarchar(max), [AssessOtherProblems] nvarchar(max), [C2_Concerns] nvarchar(max)) j6
OUTER APPLY OPENJSON(r.[BirthPlanJson]) WITH ([C2_PlanDeliver] nvarchar(max), [DevelopBirthPlan] nvarchar(max)) j7;

UPDATE r SET
    r.[ClientMI] = j1.[ClientMI],
    r.[ClientEducation] = j1.[ClientEducation],
    r.[AddressNo] = j1.[AddressNo],
    r.[AddressBarangay] = j1.[AddressBarangay],
    r.[AddressMunicipality] = j1.[AddressMunicipality],
    r.[AddressProvince] = j1.[AddressProvince],
    r.[SpouseLastName] = j1.[SpouseLastName],
    r.[SpouseGivenName] = j1.[SpouseGivenName],
    r.[SpouseMI] = j1.[SpouseMI],
    r.[SpouseDateOfBirth] = j1.[SpouseDateOfBirth],
    r.[SpouseAge] = j1.[SpouseAge],
    r.[SpouseOccupation] = j1.[SpouseOccupation],
    r.[NoOfLivingChildren] = j1.[NoOfLivingChildren],
    r.[PlanMoreChildren] = j1.[PlanMoreChildren],
    r.[AverageMonthlyIncome] = j1.[AverageMonthlyIncome],
    r.[TypeOfClient] = j2.[TypeOfClient],
    r.[ReasonForFP] = j2.[ReasonForFP],
    r.[ReasonForFP_Others] = j2.[ReasonForFP_Others],
    r.[ReasonChanging] = j2.[ReasonChanging],
    r.[MethodCurrentlyUsed] = j2.[MethodCurrentlyUsed],
    r.[MethodCurrentlyUsed_Others] = j2.[MethodCurrentlyUsed_Others],
    r.[MH_SevereHeadaches] = j3.[MH_SevereHeadaches],
    r.[MH_StrokeHeartHypertension] = j3.[MH_StrokeHeartHypertension],
    r.[MH_HematomaBruising] = j3.[MH_HematomaBruising],
    r.[MH_BreastCancerMass] = j3.[MH_BreastCancerMass],
    r.[MH_SevereChestPain] = j3.[MH_SevereChestPain],
    r.[MH_Cough14Days] = j3.[MH_Cough14Days],
    r.[MH_Jaundice] = j3.[MH_Jaundice],
    r.[MH_UnexplainedVaginalBleeding] = j3.[MH_UnexplainedVaginalBleeding],
    r.[MH_AbnormalVaginalDischarge] = j3.[MH_AbnormalVaginalDischarge],
    r.[MH_PhenobarbitalRifampicin] = j3.[MH_PhenobarbitalRifampicin],
    r.[MH_Smoker] = j3.[MH_Smoker],
    r.[MH_WithDisability] = j3.[MH_WithDisability],
    r.[MH_DisabilitySpecify] = j3.[MH_DisabilitySpecify],
    r.[OH_Gravida] = j4.[OH_Gravida],
    r.[OH_Para] = j4.[OH_Para],
    r.[OH_FullTerm] = j4.[OH_FullTerm],
    r.[OH_Premature] = j4.[OH_Premature],
    r.[OH_Abortion] = j4.[OH_Abortion],
    r.[OH_LivingChildren] = j4.[OH_LivingChildren],
    r.[OH_DateOfLastDelivery] = j4.[OH_DateOfLastDelivery],
    r.[OH_TypeOfLastDelivery] = j4.[OH_TypeOfLastDelivery],
    r.[OH_LastMenstrualPeriod] = j4.[OH_LastMenstrualPeriod],
    r.[OH_PreviousMenstrualPeriod] = j4.[OH_PreviousMenstrualPeriod],
    r.[OH_MenstrualFlow] = j4.[OH_MenstrualFlow],
    r.[OH_Dysmenorrhea] = j4.[OH_Dysmenorrhea],
    r.[OH_HydatidiformMole] = j4.[OH_HydatidiformMole],
    r.[OH_EctopicPregnancy] = j4.[OH_EctopicPregnancy],
    r.[STI_AbnormalDischarge] = j5.[STI_AbnormalDischarge],
    r.[STI_AbnormalDischarge_Loc] = j5.[STI_AbnormalDischarge_Loc],
    r.[STI_SoresUlcers] = j5.[STI_SoresUlcers],
    r.[STI_PainBurning] = j5.[STI_PainBurning],
    r.[STI_HistoryTreatment] = j5.[STI_HistoryTreatment],
    r.[STI_HIV_PID] = j5.[STI_HIV_PID],
    r.[VAW_UnpleasantRelationship] = j5.[VAW_UnpleasantRelationship],
    r.[VAW_PartnerDisapprove] = j5.[VAW_PartnerDisapprove],
    r.[VAW_HistoryDomesticViolence] = j5.[VAW_HistoryDomesticViolence],
    r.[VAW_ReferredTo] = j5.[VAW_ReferredTo],
    r.[VAW_ReferredTo_Others] = j5.[VAW_ReferredTo_Others],
    r.[PE_Height] = j6.[PE_Height],
    r.[PE_Weight] = j6.[PE_Weight],
    r.[PE_BloodPressure_Systolic] = j6.[PE_BloodPressure_Systolic],
    r.[PE_BloodPressure_Diastolic] = j6.[PE_BloodPressure_Diastolic],
    r.[PE_PulseRate] = j6.[PE_PulseRate],
    r.[PE_Skin] = j6.[PE_Skin],
    r.[PE_Conjunctiva] = j6.[PE_Conjunctiva],
    r.[PE_Neck] = j6.[PE_Neck],
    r.[PE_Breast] = j6.[PE_Breast],
    r.[PE_Abdomen] = j6.[PE_Abdomen],
    r.[PE_Extremities] = j6.[PE_Extremities],
    r.[Pelvic_Normal] = j6.[Pelvic_Normal],
    r.[Pelvic_Mass] = j6.[Pelvic_Mass],
    r.[Pelvic_AbnormalDischarge] = j6.[Pelvic_AbnormalDischarge],
    r.[Pelvic_CervicalAbnormalities] = j6.[Pelvic_CervicalAbnormalities],
    r.[Pelvic_CervicalConsistency] = j6.[Pelvic_CervicalConsistency],
    r.[Pelvic_CervicalTenderness] = j6.[Pelvic_CervicalTenderness],
    r.[Pelvic_AdnexalMassTenderness] = j6.[Pelvic_AdnexalMassTenderness],
    r.[Pelvic_UterinePosition] = j6.[Pelvic_UterinePosition],
    r.[Pelvic_UterineDepth] = j6.[Pelvic_UterineDepth],
    r.[Ack_ClientPrintedName] = j7.[Ack_ClientPrintedName],
    r.[Ack_ClientDate] = j7.[Ack_ClientDate],
    r.[Ack_ConsentName] = j7.[Ack_ConsentName],
    r.[Ack_ParentPrintedName] = j7.[Ack_ParentPrintedName],
    r.[Ack_ParentDate] = j7.[Ack_ParentDate]
FROM [FamilyPlanningRecords] r
OUTER APPLY OPENJSON(r.[ClientInfoJson]) WITH ([ClientMI] nvarchar(max), [ClientEducation] nvarchar(max), [AddressNo] nvarchar(max), [AddressBarangay] nvarchar(max), [AddressMunicipality] nvarchar(max), [AddressProvince] nvarchar(max), [SpouseLastName] nvarchar(max), [SpouseGivenName] nvarchar(max), [SpouseMI] nvarchar(max), [SpouseDateOfBirth] datetime2, [SpouseAge] nvarchar(max), [SpouseOccupation] nvarchar(max), [NoOfLivingChildren] nvarchar(max), [PlanMoreChildren] nvarchar(max), [AverageMonthlyIncome] nvarchar(max)) j1
OUTER APPLY OPENJSON(r.[FamilyPlanningMethodJson]) WITH ([TypeOfClient] nvarchar(max), [ReasonForFP] nvarchar(max), [ReasonForFP_Others] nvarchar(max), [ReasonChanging] nvarchar(max), [MethodCurrentlyUsed] nvarchar(max), [MethodCurrentlyUsed_Others] nvarchar(max)) j2
OUTER APPLY OPENJSON(r.[MedicalHistoryJson]) WITH ([MH_SevereHeadaches] nvarchar(max), [MH_StrokeHeartHypertension] nvarchar(max), [MH_HematomaBruising] nvarchar(max), [MH_BreastCancerMass] nvarchar(max), [MH_SevereChestPain] nvarchar(max), [MH_Cough14Days] nvarchar(max), [MH_Jaundice] nvarchar(max), [MH_UnexplainedVaginalBleeding] nvarchar(max), [MH_AbnormalVaginalDischarge] nvarchar(max), [MH_PhenobarbitalRifampicin] nvarchar(max), [MH_Smoker] nvarchar(max), [MH_WithDisability] nvarchar(max), [MH_DisabilitySpecify] nvarchar(max)) j3
OUTER APPLY OPENJSON(r.[ObstetricalHistoryJson]) WITH ([OH_Gravida] nvarchar(max), [OH_Para] nvarchar(max), [OH_FullTerm] nvarchar(max), [OH_Premature] nvarchar(max), [OH_Abortion] nvarchar(max), [OH_LivingChildren] nvarchar(max), [OH_DateOfLastDelivery] datetime2, [OH_TypeOfLastDelivery] nvarchar(max), [OH_LastMenstrualPeriod] datetime2, [OH_PreviousMenstrualPeriod] datetime2, [OH_MenstrualFlow] nvarchar(max), [OH_Dysmenorrhea] nvarchar(max), [OH_HydatidiformMole] nvarchar(max), [OH_EctopicPregnancy] nvarchar(max)) j4
OUTER APPLY OPENJSON(r.[RiskAssessmentJson]) WITH ([STI_AbnormalDischarge] nvarchar(max), [STI_AbnormalDischarge_Loc] nvarchar(max), [STI_SoresUlcers] nvarchar(max), [STI_PainBurning] nvarchar(max), [STI_HistoryTreatment] nvarchar(max), [STI_HIV_PID] nvarchar(max), [VAW_UnpleasantRelationship] nvarchar(max), [VAW_PartnerDisapprove] nvarchar(max), [VAW_HistoryDomesticViolence] nvarchar(max), [VAW_ReferredTo] nvarchar(max), [VAW_ReferredTo_Others] nvarchar(max)) j5
OUTER APPLY OPENJSON(r.[PhysicalExamJson]) WITH ([PE_Height] nvarchar(max), [PE_Weight] nvarchar(max), [PE_BloodPressure_Systolic] nvarchar(max), [PE_BloodPressure_Diastolic] nvarchar(max), [PE_PulseRate] nvarchar(max), [PE_Skin] nvarchar(max), [PE_Conjunctiva] nvarchar(max), [PE_Neck] nvarchar(max), [PE_Breast] nvarchar(max), [PE_Abdomen] nvarchar(max), [PE_Extremities] nvarchar(max), [Pelvic_Normal] nvarchar(max), [Pelvic_Mass] nvarchar(max), [Pelvic_AbnormalDischarge] nvarchar(max), [Pelvic_CervicalAbnormalities] nvarchar(max), [Pelvic_CervicalConsistency] nvarchar(max), [Pelvic_CervicalTenderness] nvarchar(max), [Pelvic_AdnexalMassTenderness] nvarchar(max), [Pelvic_UterinePosition] nvarchar(max), [Pelvic_UterineDepth] nvarchar(max)) j6
OUTER APPLY OPENJSON(r.[AcknowledgementJson]) WITH ([Ack_ClientPrintedName] nvarchar(max), [Ack_ClientDate] datetime2, [Ack_ConsentName] nvarchar(max), [Ack_ParentPrintedName] nvarchar(max), [Ack_ParentDate] datetime2) j7;
");

            migrationBuilder.DropColumn(
                name: "BirthPlanJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "GeneralInfoJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "LaboratoryJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "MaternalHistoryJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "ObstetricInfoJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "RiskAssessmentJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "VitalSignsJson",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "AcknowledgementJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientInfoJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "FamilyPlanningMethodJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "MedicalHistoryJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ObstetricalHistoryJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "PhysicalExamJson",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "RiskAssessmentJson",
                table: "FamilyPlanningRecords");

        }
    }
}
