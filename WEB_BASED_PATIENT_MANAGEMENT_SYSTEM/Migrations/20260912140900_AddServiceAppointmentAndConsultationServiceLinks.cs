using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAppointmentAndConsultationServiceLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Consultations",
                type: "int",
                nullable: true);

            // One-time backfill for pre-existing rows: pair each Service to the
            // Appointment it was actually created for, oldest-to-oldest, matched
            // by (PatientId, ServiceName/ServiceType). Any Service left unmatched
            // (no corresponding Appointment) is a genuine pre-existing Walk-In and
            // is correctly left NULL. New rows are set directly by application
            // code going forward and never rely on this heuristic.
            migrationBuilder.Sql(@"
                ;WITH RankedServices AS (
                    SELECT s.Id AS ServiceId, s.PatientId, s.ServiceName,
                           ROW_NUMBER() OVER (PARTITION BY s.PatientId, s.ServiceName ORDER BY s.Id) AS rn
                    FROM [Service] s
                ),
                RankedAppointments AS (
                    SELECT a.Id AS AppointmentId, a.PatientId, a.ServiceType,
                           ROW_NUMBER() OVER (PARTITION BY a.PatientId, a.ServiceType ORDER BY a.Id) AS rn
                    FROM [Appointments] a
                    WHERE a.ServiceType IS NOT NULL AND a.ServiceType <> ''
                )
                UPDATE svc
                SET svc.AppointmentId = ra.AppointmentId
                FROM [Service] svc
                JOIN RankedServices rs ON rs.ServiceId = svc.Id
                JOIN RankedAppointments ra
                    ON ra.PatientId = rs.PatientId
                    AND ra.ServiceType = rs.ServiceName
                    AND ra.rn = rs.rn;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Service_AppointmentId",
                table: "Service",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ServiceId",
                table: "Consultations",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Service_ServiceId",
                table: "Consultations",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Appointments_AppointmentId",
                table: "Service",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Service_ServiceId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_Appointments_AppointmentId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Service_AppointmentId",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_ServiceId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Consultations");
        }
    }
}
