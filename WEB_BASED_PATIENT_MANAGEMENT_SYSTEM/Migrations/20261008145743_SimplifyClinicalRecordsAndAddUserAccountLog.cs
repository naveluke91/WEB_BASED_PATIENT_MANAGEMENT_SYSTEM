using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <summary>
    /// Removes redundant columns from PrenatalRecords, NewbornRecords and
    /// FamilyPlanningRecords (copies of Patient / Consultation data, or columns
    /// nothing uses), tags the remaining PrenatalRecords / FamilyPlanningRecords
    /// columns with a category (MS_Description), and adds user-log columns to
    /// UserAccounts. No table is added or removed.
    /// </summary>
    public partial class SimplifyClinicalRecordsAndAddUserAccountLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some databases have PrenatalRecords / FamilyPlanningRecords tables that
            // were re-created outside EF Core: their primary keys have system-generated
            // names and IX_PrenatalRecords_PatientId is missing. Align them with the
            // model. On a database built only by migrations these statements do nothing.
            migrationBuilder.Sql(@"
DECLARE @pk nvarchar(300);

SELECT @pk = QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name)
FROM sys.key_constraints
WHERE type = 'PK' AND parent_object_id = OBJECT_ID(N'[PrenatalRecords]') AND name <> N'PK_PrenatalRecords';
IF @pk IS NOT NULL EXEC sp_rename @pk, N'PK_PrenatalRecords', N'OBJECT';

SET @pk = NULL;
SELECT @pk = QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name)
FROM sys.key_constraints
WHERE type = 'PK' AND parent_object_id = OBJECT_ID(N'[FamilyPlanningRecords]') AND name <> N'PK_FamilyPlanningRecords';
IF @pk IS NOT NULL EXEC sp_rename @pk, N'PK_FamilyPlanningRecords', N'OBJECT';

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'[PrenatalRecords]') AND name = N'IX_PrenatalRecords_PatientId')
    CREATE INDEX [IX_PrenatalRecords_PatientId] ON [PrenatalRecords] ([PatientId]);
");

            // Columns below are dropped. Their values are not lost: the app now reads
            // them from Patient (or Consultation.ServiceType) through the same
            // properties. PatientAddress, FundalHeight, FetalHeartTone, Remarks
            // (PrenatalRecords) and ClientIdStr (FamilyPlanningRecords) were never used.
            migrationBuilder.DropColumn(
                name: "Address",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_EDC",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "C2_LMP",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "ContactNo",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "EDC",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "FetalHeartTone",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "FundalHeight",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "G",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Gravida",
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
                name: "PatientAddress",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "SelectedServices",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "TFAL",
                table: "PrenatalRecords");

            migrationBuilder.DropColumn(
                name: "MotherAddress",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "Ack_MethodAccepted",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "CivilStatus",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientAge",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientDateOfBirth",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientGivenName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientIdStr",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientLastName",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ClientOccupation",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "ContactNo",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropColumn(
                name: "SelectedService",
                table: "FamilyPlanningRecords");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAtUtc",
                table: "UserAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAtUtc",
                table: "UserAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogoutAtUtc",
                table: "UserAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VisitType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ThirdTrimester_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ThirdTrimester_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "T",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "date",
                nullable: false,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrenatalVisitsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "P",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "L",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HusbandName",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Delivery / Follow-up",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_Hemoglobin",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Laboratory / Diagnostic Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_ConjunctivalPallor",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C3_PreEclampsia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C3_PreEclampsia_BP",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_VaginalBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_TT4",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_Stillbirth",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_SmokeDrinkDrugs",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorTear",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorPregnancies",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorCaesarian",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PreviousComplications",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_Presentation",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PregnancyWeek",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_PlanDeliver",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Delivery / Follow-up",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_MonthsPregnant",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_FHB",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_Convulsions",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_Concerns",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_BabyMoving",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "C2_BabyBefore",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Maternal History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Laboratory / Diagnostic Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BloodPressure",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "Category: Vital Signs / Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B3_RAM_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B3_RAM_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B2_QuickCheck_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "B2_QuickCheck_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssessOtherProblems",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy Risk / Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AntenatalDate",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: General Prenatal Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AOG",
                table: "PrenatalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "A",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Pregnancy / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_UnpleasantRelationship",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_ReferredTo_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_ReferredTo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_PartnerDisapprove",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_HistoryDomesticViolence",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfClient",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpouseOccupation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpouseMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpouseLastName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpouseGivenName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SpouseDateOfBirth",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpouseAge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_SoresUlcers",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_PainBurning",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_HistoryTreatment",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_HIV_PID",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_AbnormalDischarge_Loc",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "STI_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Counseling / Risk Assessment (STI, VAW)",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForFP_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForFP",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReasonChanging",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlanMoreChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_UterinePosition",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_UterineDepth",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_Normal",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_Mass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalConsistency",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalAbnormalities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_AdnexalMassTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Weight",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Skin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_PulseRate",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Neck",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Height",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Extremities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Conjunctiva",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Breast",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_BloodPressure_Systolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_BloodPressure_Diastolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PE_Abdomen",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Physical Examination",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_TypeOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_PreviousMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_Premature",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_Para",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_MenstrualFlow",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_LivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_LastMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_HydatidiformMole",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_Gravida",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_FullTerm",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_EctopicPregnancy",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_Dysmenorrhea",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_DateOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OH_Abortion",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoOfLivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Reproductive / Obstetric Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MethodCurrentlyUsed_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MethodCurrentlyUsed",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Family Planning Method / History",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_WithDisability",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_UnexplainedVaginalBleeding",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_StrokeHeartHypertension",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_Smoker",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_SevereHeadaches",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_SevereChestPain",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_PhenobarbitalRifampicin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_Jaundice",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_HematomaBruising",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_DisabilitySpecify",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_Cough14Days",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_BreastCancerMass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MH_AbnormalVaginalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Medical / Health Assessment",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientEducation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AverageMonthlyIncome",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressProvince",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressNo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressMunicipality",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressBarangay",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Client Information",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ParentPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Ack_ParentDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Acknowledgement / Consent",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ConsentName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ClientPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Category: Acknowledgement / Consent",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Ack_ClientDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                comment: "Category: Acknowledgement / Consent",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivityAtUtc",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "LastLoginAtUtc",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "LastLogoutAtUtc",
                table: "UserAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldComment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "VisitType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "ThirdTrimester_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "ThirdTrimester_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "T",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "PrioritySigns_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "PrenatalVisitsJson",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_YesNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "PatientProblems_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "P",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "L",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "HusbandName",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNo",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "DevelopBirthPlan",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Delivery / Follow-up");

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_Hemoglobin",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Laboratory / Diagnostic Information");

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_ConjunctivalPallor",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "C4_Anemia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "C3_PreEclampsia_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "C3_PreEclampsia_BP",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "C2_VaginalBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_TT4",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_Stillbirth",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_SmokeDrinkDrugs",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorTear",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorPregnancies",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PriorCaesarian",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PreviousComplications",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_Presentation",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PregnancyWeek",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_PlanDeliver",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Delivery / Follow-up");

            migrationBuilder.AlterColumn<string>(
                name: "C2_MonthsPregnant",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_HeavyBleeding",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_FHB",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_Convulsions",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "C2_Concerns",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "C2_BabyMoving",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "C2_BabyBefore",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Maternal History");

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Laboratory / Diagnostic Information");

            migrationBuilder.AlterColumn<string>(
                name: "BloodPressure",
                table: "PrenatalRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Category: Vital Signs / Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "B3_RAM_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "B3_RAM_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "B2_QuickCheck_Classify",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "B2_QuickCheck_Checkboxes",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "AssessOtherProblems",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy Risk / Assessment");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AntenatalDate",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: General Prenatal Information");

            migrationBuilder.AlterColumn<string>(
                name: "AOG",
                table: "PrenatalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "A",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Pregnancy / Obstetric Information");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "C2_EDC",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "C2_LMP",
                table: "PrenatalRecords",
                type: "datetime2",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "EDC",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FetalHeartTone",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundalHeight",
                table: "PrenatalRecords",
                type: "nvarchar(30)",
                maxLength: 30,
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
                name: "Remarks",
                table: "PrenatalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedServices",
                table: "PrenatalRecords",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TFAL",
                table: "PrenatalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherAddress",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VAW_UnpleasantRelationship",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "VAW_ReferredTo_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "VAW_ReferredTo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "VAW_PartnerDisapprove",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "VAW_HistoryDomesticViolence",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfClient",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "SpouseOccupation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "SpouseMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "SpouseLastName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "SpouseGivenName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SpouseDateOfBirth",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "SpouseAge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "STI_SoresUlcers",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "STI_PainBurning",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "STI_HistoryTreatment",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "STI_HIV_PID",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "STI_AbnormalDischarge_Loc",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<string>(
                name: "STI_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Counseling / Risk Assessment (STI, VAW)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForFP_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonForFP",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonChanging",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "PlanMoreChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_UterinePosition",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_UterineDepth",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_Normal",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_Mass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalConsistency",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_CervicalAbnormalities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_AdnexalMassTenderness",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "Pelvic_AbnormalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Weight",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Skin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_PulseRate",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Neck",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Height",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Extremities",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Conjunctiva",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Breast",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_BloodPressure_Systolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_BloodPressure_Diastolic",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "PE_Abdomen",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Physical Examination");

            migrationBuilder.AlterColumn<string>(
                name: "OH_TypeOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_PreviousMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_Premature",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_Para",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_MenstrualFlow",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_LivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_LastMenstrualPeriod",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_HydatidiformMole",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_Gravida",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_FullTerm",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_EctopicPregnancy",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_Dysmenorrhea",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OH_DateOfLastDelivery",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "OH_Abortion",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "NoOfLivingChildren",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Reproductive / Obstetric Information");

            migrationBuilder.AlterColumn<string>(
                name: "MethodCurrentlyUsed_Others",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "MethodCurrentlyUsed",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Family Planning Method / History");

            migrationBuilder.AlterColumn<string>(
                name: "MH_WithDisability",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_UnexplainedVaginalBleeding",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_StrokeHeartHypertension",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_Smoker",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_SevereHeadaches",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_SevereChestPain",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_PhenobarbitalRifampicin",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_Jaundice",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_HematomaBruising",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_DisabilitySpecify",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_Cough14Days",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_BreastCancerMass",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "MH_AbnormalVaginalDischarge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Medical / Health Assessment");

            migrationBuilder.AlterColumn<string>(
                name: "ClientMI",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "ClientEducation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "AverageMonthlyIncome",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "AddressProvince",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "AddressNo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "AddressMunicipality",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "AddressBarangay",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Client Information");

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ParentPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Acknowledgement / Consent");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Ack_ParentDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Acknowledgement / Consent");

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ConsentName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Acknowledgement / Consent");

            migrationBuilder.AlterColumn<string>(
                name: "Ack_ClientPrintedName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Category: Acknowledgement / Consent");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Ack_ClientDate",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldComment: "Category: Acknowledgement / Consent");

            migrationBuilder.AddColumn<string>(
                name: "Ack_MethodAccepted",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilStatus",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientAge",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientDateOfBirth",
                table: "FamilyPlanningRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientGivenName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientIdStr",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientLastName",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientOccupation",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNo",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedService",
                table: "FamilyPlanningRecords",
                type: "nvarchar(max)",
                nullable: true);

            // Refill the restored copy columns from Patient / Consultation, so after a
            // rollback the old schema holds the same values the app was showing.
            migrationBuilder.Sql(@"
UPDATE r SET
    r.PatientName = p.FullName, r.Address = p.Address, r.DateOfBirth = p.DateOfBirth,
    r.MaritalStatus = p.MaritalStatus, r.LMP = p.LMP, r.EDC = p.EDC, r.Menarche = p.Menarche,
    r.ContactNo = p.ContactNo, r.Gravida = p.Gravida, r.TFAL = p.TFAL, r.Occupation = p.Occupation,
    r.G = p.Gravida, r.C2_LMP = p.LMP, r.C2_EDC = p.EDC
FROM [PrenatalRecords] r
JOIN [Patient] p ON p.Id = r.PatientId;

UPDATE r SET r.SelectedServices = c.ServiceType
FROM [PrenatalRecords] r
JOIN [Consultations] c ON c.RecordType = N'Prenatal' AND c.RecordId = r.Id;

UPDATE r SET r.MotherName = p.FullName, r.MotherAddress = p.Address
FROM [NewbornRecords] r
JOIN [Patient] p ON p.Id = r.PatientId;

UPDATE r SET
    r.ClientLastName = CASE WHEN s.Pos = 0 THEN n.FullName ELSE RIGHT(n.FullName, s.Pos - 1) END,
    r.ClientGivenName = CASE WHEN s.Pos = 0 THEN n.FullName ELSE RTRIM(LEFT(n.FullName, LEN(n.FullName) - s.Pos)) END,
    r.ClientDateOfBirth = p.DateOfBirth, r.ClientAge = CAST(p.Age AS nvarchar(10)),
    r.ClientOccupation = p.Occupation, r.AddressStreet = p.Address, r.ContactNo = p.ContactNo,
    r.CivilStatus = p.MaritalStatus, r.Religion = p.Religion
FROM [FamilyPlanningRecords] r
JOIN [Patient] p ON p.Id = r.PatientId
CROSS APPLY (SELECT LTRIM(RTRIM(p.FullName)) AS FullName) n
CROSS APPLY (SELECT CHARINDEX(N' ', REVERSE(n.FullName)) AS Pos) s;

UPDATE r SET r.SelectedService = c.ServiceType, r.Ack_MethodAccepted = c.ServiceType
FROM [FamilyPlanningRecords] r
JOIN [Consultations] c ON c.RecordType = N'Family Planning' AND c.RecordId = r.Id;
");
        }
    }
}
