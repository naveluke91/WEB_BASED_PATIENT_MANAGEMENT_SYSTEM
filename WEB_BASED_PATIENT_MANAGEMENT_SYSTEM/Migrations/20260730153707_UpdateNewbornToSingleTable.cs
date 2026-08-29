using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewbornToSingleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewbornMedications");

            migrationBuilder.DropTable(
                name: "NewbornVitals");

            migrationBuilder.AddColumn<string>(
                name: "MedicationsJson",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitalsJson",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicationsJson",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "VitalsJson",
                table: "NewbornRecords");

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
                name: "IX_NewbornVitals_NewbornRecordId",
                table: "NewbornVitals",
                column: "NewbornRecordId");
        }
    }
}
