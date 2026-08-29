using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class AddNewbornRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "PrenatalRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LMP",
                table: "Patients",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EDC",
                table: "Patients",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "NewbornRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    CaseNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeOfAdmission = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BabyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeDelivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BedNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlacentaOut = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApgarScore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasurementHead = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasurementChest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasurementAbdomen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasurementLength = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeOfDischarge = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsentGivenBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentMidwifeName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewbornRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewbornRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NewbornMedications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewbornRecordId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MedicineGiven = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewbornMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewbornMedications_NewbornRecords_NewbornRecordId",
                        column: x => x.NewbornRecordId,
                        principalTable: "NewbornRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewbornVitals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewbornRecordId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HeartRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RespiratoryRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewbornVitals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewbornVitals_NewbornRecords_NewbornRecordId",
                        column: x => x.NewbornRecordId,
                        principalTable: "NewbornRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewbornMedications_NewbornRecordId",
                table: "NewbornMedications",
                column: "NewbornRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_NewbornRecords_PatientId",
                table: "NewbornRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewbornVitals_NewbornRecordId",
                table: "NewbornVitals",
                column: "NewbornRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords");

            migrationBuilder.DropTable(
                name: "NewbornMedications");

            migrationBuilder.DropTable(
                name: "NewbornVitals");

            migrationBuilder.DropTable(
                name: "NewbornRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "PrenatalRecords",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "PrenatalRecords",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LMP",
                table: "Patients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EDC",
                table: "Patients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddForeignKey(
                name: "FK_PrenatalRecords_Patients_PatientId",
                table: "PrenatalRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
