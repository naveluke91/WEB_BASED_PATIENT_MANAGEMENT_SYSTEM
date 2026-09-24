using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services
{
    /// <summary>
    /// Runs in the background so a Rescheduled appointment becomes Pending once its new
    /// date and time arrive, even if nobody opens the Appointments page.
    /// </summary>
    public class RescheduledAppointmentStatusService : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RescheduledAppointmentStatusService> _logger;

        public RescheduledAppointmentStatusService(IServiceScopeFactory scopeFactory, ILogger<RescheduledAppointmentStatusService> logger)
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
                    var now = DateTime.Now;
                    var today = now.Date;
                    var timeNow = now.TimeOfDay;

                    await db.Appointments
                        .Where(a => a.Status == "Rescheduled"
                            && (a.AppointmentDate < today || (a.AppointmentDate == today && a.AppointmentTime <= timeNow)))
                        .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, "Pending"), stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Rescheduled appointments could not be changed to Pending.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
