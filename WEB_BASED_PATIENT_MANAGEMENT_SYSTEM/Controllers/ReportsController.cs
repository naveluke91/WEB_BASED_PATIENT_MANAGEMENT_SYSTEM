using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// Reports → Generate Reports. Read-only summaries of the existing patient,
    /// appointment, consultation and payment records, for Admin and Staff.
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------------
        // GET /Reports
        // GET /Reports?reportType=Appointment&dateFrom=2026-09-01&dateTo=2026-09-30
        // -----------------------------------------------------------------------
        public IActionResult Index(string? reportType, DateTime? dateFrom, DateTime? dateTo)
        {
            var model = new ReportsPageViewModel
            {
                ReportType = reportType,
                DateFrom = dateFrom?.Date,
                DateTo = dateTo?.Date
            };

            // Opening the page shows only the filters.
            if (string.IsNullOrWhiteSpace(reportType))
                return View(model);

            if (!ReportsPageViewModel.ReportTypes.Any(type => type.Value == reportType))
                model.ErrorMessage = "Please select a report type.";
            else if (!ModelState.IsValid)
                model.ErrorMessage = "Please enter valid dates.";
            else if (model.DateFrom > model.DateTo)
                model.ErrorMessage = "Date From must be on or before Date To.";

            if (model.ErrorMessage != null)
                return View(model);

            // Inclusive range: from the start of Date From to the end of Date To.
            // A blank date leaves that side of the range open.
            var from = model.DateFrom;
            var toExclusive = model.DateTo?.AddDays(1);

            if (reportType == ReportsPageViewModel.PatientReport)
            {
                var patients = _context.Patients
                    .AsNoTracking()
                    .AsQueryable();

                if (from.HasValue)
                    patients = patients.Where(p => p.DateRegistered >= from.Value);
                if (toExclusive.HasValue)
                    patients = patients.Where(p => p.DateRegistered < toExclusive.Value);

                model.Patients = patients
                    .OrderBy(p => p.FullName)
                    .ToList();
            }
            else if (reportType == ReportsPageViewModel.AppointmentReport)
            {
                var appointments = _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .AsQueryable();

                if (from.HasValue)
                    appointments = appointments.Where(a => a.AppointmentDate >= from.Value);
                if (toExclusive.HasValue)
                    appointments = appointments.Where(a => a.AppointmentDate < toExclusive.Value);

                model.Appointments = appointments
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToList();
            }
            else if (reportType == ReportsPageViewModel.ConsultationReport)
            {
                var consultations = _context.Consultations
                    .AsNoTracking()
                    .Include(c => c.Patient)
                    .AsQueryable();

                if (from.HasValue)
                    consultations = consultations.Where(c => c.StartedAt >= from.Value);
                if (toExclusive.HasValue)
                    consultations = consultations.Where(c => c.StartedAt < toExclusive.Value);

                model.Consultations = consultations
                    .OrderByDescending(c => c.StartedAt)
                    .ToList();
            }
            else
            {
                var payments = _context.Payments
                    .AsNoTracking()
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Patient)
                    .AsQueryable();

                if (from.HasValue)
                    payments = payments.Where(p => p.PaymentDate >= from.Value);
                if (toExclusive.HasValue)
                    payments = payments.Where(p => p.PaymentDate < toExclusive.Value);

                model.Payments = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList();
            }

            model.IsGenerated = true;
            model.GeneratedAt = DateTime.Now;
            return View(model);
        }
    }
}
