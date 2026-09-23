using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoveryEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecoveryEmailVerified",
                table: "UserAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Grandfather accounts that already have a Recovery Email on file (set before this
            // verification requirement existed) so Forgot Password keeps working for them.
            migrationBuilder.Sql(
                "UPDATE [UserAccounts] SET [IsRecoveryEmailVerified] = 1 WHERE [RecoveryEmail] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecoveryEmailVerified",
                table: "UserAccounts");
        }
    }
}
