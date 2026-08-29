using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewbornConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConsentGivenBy",
                table: "NewbornRecords",
                newName: "ConsentRelationship");

            migrationBuilder.AddColumn<string>(
                name: "ConsentCivilStatus",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentClientAddress",
                table: "NewbornRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsentClientAge",
                table: "NewbornRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentClientDate",
                table: "NewbornRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentMidwifeDate",
                table: "NewbornRecords",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentCivilStatus",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "ConsentClientAddress",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "ConsentClientAge",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "ConsentClientDate",
                table: "NewbornRecords");

            migrationBuilder.DropColumn(
                name: "ConsentMidwifeDate",
                table: "NewbornRecords");

            migrationBuilder.RenameColumn(
                name: "ConsentRelationship",
                table: "NewbornRecords",
                newName: "ConsentGivenBy");
        }
    }
}
