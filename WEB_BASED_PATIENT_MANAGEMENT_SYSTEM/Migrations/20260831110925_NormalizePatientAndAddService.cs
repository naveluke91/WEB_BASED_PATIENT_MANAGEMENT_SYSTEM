using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePatientAndAddService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Patients_PatientId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyPlanningRecords_Patients_PatientId",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_NewbornRecords_Patients_PatientId",
                table: "NewbornRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords");

            // The original live database used an auto-generated primary-key
            // name instead of PK_Patients. Find it dynamically so the rename
            // works for both that database and a normal EF-created database.
            migrationBuilder.Sql(@"
                DECLARE @primaryKeyName sysname;
                SELECT @primaryKeyName = [kc].[name]
                FROM [sys].[key_constraints] AS [kc]
                INNER JOIN [sys].[tables] AS [t] ON [t].[object_id] = [kc].[parent_object_id]
                WHERE [kc].[type] = 'PK' AND [t].[name] = N'Patients';

                IF @primaryKeyName IS NOT NULL
                BEGIN
                    DECLARE @dropPrimaryKeySql nvarchar(max) =
                        N'ALTER TABLE [Patients] DROP CONSTRAINT ' + QUOTENAME(@primaryKeyName) + N';';
                    EXEC(@dropPrimaryKeySql);
                END;");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "Patient");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patient",
                table: "Patient",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Service_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Service_PatientId",
                table: "Service",
                column: "PatientId");

            // Preserve service choices that were previously stored in clinical
            // records.  The Service table is now the source of the selected
            // patient + service pair; the clinical records themselves remain
            // unchanged for the Consultation forms.
            migrationBuilder.Sql(@"
                INSERT INTO [Service] ([PatientId], [ServiceName], [Price])
                SELECT [PatientId],
                       COALESCE(NULLIF(LTRIM(RTRIM([SelectedServices])), ''), 'Prenatal'),
                       CAST(100.00 AS decimal(10,2))
                FROM [PrenatalRecords]
                WHERE [PatientId] IS NOT NULL;

                INSERT INTO [Service] ([PatientId], [ServiceName], [Price])
                SELECT [PatientId],
                       'Normal Delivery Fee & Newborn Care Package',
                       CAST(15000.00 AS decimal(10,2))
                FROM [NewbornRecords]
                WHERE [PatientId] IS NOT NULL;

                INSERT INTO [Service] ([PatientId], [ServiceName], [Price])
                SELECT [PatientId],
                       LTRIM(RTRIM([SelectedService])),
                       CAST(CASE LTRIM(RTRIM([SelectedService]))
                           WHEN 'Implant' THEN 0.00
                           WHEN 'Implant Removal' THEN 700.00
                           WHEN 'DEPO' THEN 200.00
                           WHEN 'NORIFAM' THEN 410.00
                           WHEN 'IUD Insertion' THEN 0.00
                           WHEN 'IUD Removal' THEN 600.00
                           WHEN 'Anti-Tetanus Injection' THEN 200.00
                           ELSE 0.00
                       END AS decimal(10,2))
                FROM [FamilyPlanningRecords]
                WHERE [PatientId] IS NOT NULL
                  AND NULLIF(LTRIM(RTRIM([SelectedService])), '') IS NOT NULL;");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patient_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Patient_PatientId",
                table: "Consultations",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyPlanningRecords_Patient_PatientId",
                table: "FamilyPlanningRecords",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NewbornRecords_Patient_PatientId",
                table: "NewbornRecords",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrenatalRecords_Patient_PatientId",
                table: "PrenatalRecords",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patient_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Patient_PatientId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyPlanningRecords_Patient_PatientId",
                table: "FamilyPlanningRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_NewbornRecords_Patient_PatientId",
                table: "NewbornRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PrenatalRecords_Patient_PatientId",
                table: "PrenatalRecords");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patient",
                table: "Patient");

            migrationBuilder.RenameTable(
                name: "Patient",
                newName: "Patients");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                table: "Patients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Patients_PatientId",
                table: "Consultations",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyPlanningRecords_Patients_PatientId",
                table: "FamilyPlanningRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NewbornRecords_Patients_PatientId",
                table: "NewbornRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
