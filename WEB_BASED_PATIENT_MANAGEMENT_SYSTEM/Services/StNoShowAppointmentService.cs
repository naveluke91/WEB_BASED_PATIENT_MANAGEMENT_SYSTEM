// ==== [ST-APPT-16] START — Background service: lapas na nga Pending → "No-Show" ====
// I-register sab sa Program.cs (parehas nga #16). Matag 10 minutes, ang Pending nga
// appointment nga lapas na ang petsa (gahapon pababa) kay himuong "No-Show".
// (Ang CSS sa "No-Show" badge kay naa sa [ST-APPT-15].)
/*
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services
{
    public class StNoShowAppointmentService : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StNoShowAppointmentService> _logger;

        public StNoShowAppointmentService(IServiceScopeFactory scopeFactory, ILogger<StNoShowAppointmentService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var today = DateTime.Today;

                    await db.Appointments
                        .Where(a => a.Status == "Pending" && a.AppointmentDate < today)
                        .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, "No-Show"), stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Past pending appointments could not be changed to No-Show.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
*/
// ==== [ST-APPT-16] END ====
