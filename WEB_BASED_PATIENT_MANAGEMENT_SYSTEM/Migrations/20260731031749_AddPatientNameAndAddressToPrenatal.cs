using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientNameAndAddressToPrenatal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "PrenatalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FundalHeight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "FetalHeartTone",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "BloodPressure",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "AOG",
                table: "PrenatalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "A",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AntenatalDate",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssessOtherProblems",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B2_QuickCheck_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B2_QuickCheck_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B3_RAM_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B3_RAM_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_BabyBefore",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_BabyMoving",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_Concerns",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_Convulsions",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "C2_EDC",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_FHB",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "C2_LMP",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_MonthsPregnant",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PlanDeliver",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PregnancyWeek",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_Presentation",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PreviousComplications",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorCaesarian",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorPregnancies",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_PriorTear",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_SmokeDrinkDrugs",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_Stillbirth",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_TT4",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C2_VaginalBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C3_PreEclampsia_BP",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C3_PreEclampsia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_ConjunctivalPallor",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "C4_Anemia_Hemoglobin",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EDC",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "G",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gravida",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HusbandName",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "L",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LMP",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Menarche",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "P",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientAddress",
                table: "PrenatalRecords",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "PrenatalRecords",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientProblems_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrenatalVisitsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrioritySigns_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "T",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TFAL",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdTrimester_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdTrimester_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FamilyPlanningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    SelectedService = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientIdStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientGivenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientMI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientDateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientEducation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientOccupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressStreet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressBarangay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressMunicipality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CivilStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseGivenName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseMI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseDateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SpouseAge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpouseOccupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoOfLivingChildren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanMoreChildren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AverageMonthlyIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfClient = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonForFP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonForFP_Others = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonChanging = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodCurrentlyUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodCurrentlyUsed_Others = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_SevereHeadaches = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_StrokeHeartHypertension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_HematomaBruising = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_BreastCancerMass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_SevereChestPain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_Cough14Days = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_Jaundice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_UnexplainedVaginalBleeding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_AbnormalVaginalDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_PhenobarbitalRifampicin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_Smoker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_WithDisability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MH_DisabilitySpecify = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_Gravida = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_Para = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_FullTerm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_Premature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_Abortion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_LivingChildren = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_DateOfLastDelivery = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OH_TypeOfLastDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_LastMenstrualPeriod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OH_PreviousMenstrualPeriod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OH_MenstrualFlow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_Dysmenorrhea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_HydatidiformMole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OH_EctopicPregnancy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_AbnormalDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_AbnormalDischarge_Loc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_SoresUlcers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_PainBurning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_HistoryTreatment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STI_HIV_PID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAW_UnpleasantRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAW_PartnerDisapprove = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAW_HistoryDomesticViolence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAW_ReferredTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAW_ReferredTo_Others = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Height = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_BloodPressure_Systolic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_BloodPressure_Diastolic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_PulseRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Skin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Conjunctiva = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Neck = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Breast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Abdomen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PE_Extremities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_Normal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_Mass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_AbnormalDischarge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_CervicalAbnormalities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_CervicalConsistency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_CervicalTenderness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_AdnexalMassTenderness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_UterinePosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pelvic_UterineDepth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ack_MethodAccepted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ack_ClientPrintedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ack_ClientDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ack_ConsentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ack_ParentPrintedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ack_ParentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPlanningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyPlanningRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPlanningRecords_PatientId",
                table: "FamilyPlanningRecords",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "A",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Address",
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
                name: "C2_EDC",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_FHB",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_LMP",
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
                name: "ContactNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "EDC",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "FamilyNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "G",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Gravida",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "HusbandName",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "L",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "LMP",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Menarche",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "P",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientAddress",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientName",
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
                name: "PrenatalVisitsJson",
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
                name: "TFAL",
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

            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "PrenatalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FundalHeight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FetalHeartTone",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodPressure",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AOG",
                table: "PrenatalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
