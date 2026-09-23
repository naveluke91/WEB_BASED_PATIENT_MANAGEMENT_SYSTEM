using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSuperAdminAndRestrictRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_SingleSuperAdmin",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "UserAccounts");

            // The legacy role is removed rather than converted, preserving all
            // existing Admin and Staff accounts without creating a duplicate.
            migrationBuilder.Sql("DELETE FROM [UserAccounts] WHERE [Role] = N'SuperAdmin';");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAccounts_ValidRole",
                table: "UserAccounts",
                sql: "[Role] IN (N'Admin', N'Staff')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAccounts_ValidRole",
                table: "UserAccounts");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "UserAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                table: "UserAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_SingleSuperAdmin",
                table: "UserAccounts",
                column: "Role",
                unique: true,
                filter: "[Role] = N'SuperAdmin'");
        }
    }
}
