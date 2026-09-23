using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationReadForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tangtangon ang read-marker sa appointment/account nga na-delete na (walay gamit).
            migrationBuilder.Sql(
                "DELETE FROM [NotificationReads] " +
                "WHERE [AppointmentId] NOT IN (SELECT [Id] FROM [Appointments]) " +
                "OR [UserAccountId] NOT IN (SELECT [Id] FROM [UserAccounts]);");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReads_AppointmentId",
                table: "NotificationReads",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationReads_Appointments_AppointmentId",
                table: "NotificationReads",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationReads_UserAccounts_UserAccountId",
                table: "NotificationReads",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationReads_Appointments_AppointmentId",
                table: "NotificationReads");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationReads_UserAccounts_UserAccountId",
                table: "NotificationReads");

            migrationBuilder.DropIndex(
                name: "IX_NotificationReads_AppointmentId",
                table: "NotificationReads");
        }
    }
}
