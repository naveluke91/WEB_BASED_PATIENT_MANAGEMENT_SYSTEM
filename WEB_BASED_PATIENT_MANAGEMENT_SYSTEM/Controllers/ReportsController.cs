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

            // ==== [ST-REPORT-3] START — Services Report (gi-filter sa Date Registered sa service, kung naa) ====
            // Walay date ang Service (gawas kung naka-on ang ST-SERVICE-6), so tanan ang ipakita.
            /*
            if (reportType == "Service")
            {
                model.StServices = _context.Services.AsNoTracking()
                    .Include(s => s.Patient)
                    .OrderBy(s => s.ServiceName).ThenBy(s => s.Patient.FullName)
                    .ToList();
                model.IsGenerated = true;
                model.GeneratedAt = DateTime.Now;
                return View(model);
            }
            */
            // ==== [ST-REPORT-3] END ====

            // ==== [ST-REPORT-4] START — Expected Deliveries: pasyente nga ang EDC sulod sa Date From–To ====
            /*
            if (reportType == "Delivery")
            {
                var deliveries = _context.Patients.AsNoTracking().Where(p => p.EDC != null);
                if (from.HasValue)
                    deliveries = deliveries.Where(p => p.EDC >= from.Value);
                if (toExclusive.HasValue)
                    deliveries = deliveries.Where(p => p.EDC < toExclusive.Value);

                model.Patients = deliveries.OrderBy(p => p.EDC).ToList();
                model.IsGenerated = true;
                model.GeneratedAt = DateTime.Now;
                return View(model);
            }
            */
            // ==== [ST-REPORT-4] END ====

            // ==== [ST-REPORT-5] START — Income per Service: mga bayad sulod sa Date From–To ====
            /*
            if (reportType == "Income")
            {
                var income = _context.Payments.AsNoTracking()
                    .Include(p => p.Consultation)
                    .AsQueryable();
                if (from.HasValue)
                    income = income.Where(p => p.PaymentDate >= from.Value);
                if (toExclusive.HasValue)
                    income = income.Where(p => p.PaymentDate < toExclusive.Value);

                model.Payments = income.ToList();
                model.IsGenerated = true;
                model.GeneratedAt = DateTime.Now;
                return View(model);
            }
            */
            // ==== [ST-REPORT-5] END ====

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

            // ==== [ST-REPORT-7] START — Dugang filter: Appointment Status / Payment Method / Visit Type ====
            // Gikan sa ?stFilter= (dropdown sa Index.cshtml).
            /*
            var stFilter = Request.Query["stFilter"].ToString();
            if (!string.IsNullOrEmpty(stFilter))
            {
                model.Appointments = model.Appointments.Where(a => a.Status == stFilter).ToList();
                model.Payments = model.Payments.Where(p => p.PaymentMethod == stFilter).ToList();
                model.Consultations = model.Consultations.Where(c => c.VisitType == stFilter).ToList();
            }
            */
            // ==== [ST-REPORT-7] END ====

            model.IsGenerated = true;
            model.GeneratedAt = DateTime.Now;
            return View(model);
        }

        // ==== [ST-REPORT-1] START — GET /Reports/ExportCsv: i-download ang report as CSV (maablihan sa Excel) ====
        // Gigamit ang parehas nga Index() aron parehas ang datos sa gipakita.
        /*
        [HttpGet]
        public IActionResult ExportCsv(string? reportType, DateTime? dateFrom, DateTime? dateTo)
        {
            if (Index(reportType, dateFrom, dateTo) is not ViewResult { Model: ReportsPageViewModel model } || !model.IsGenerated)
                return RedirectToAction(nameof(Index), new { reportType, dateFrom, dateTo });

            var csv = new System.Text.StringBuilder();
            static string Cell(object? value) =>
                "\"" + (Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "").Replace("\"", "\"\"") + "\"";
            void Row(params object?[] cells) => csv.AppendLine(string.Join(",", cells.Select(Cell)));

            switch (model.ReportType)
            {
                case ReportsPageViewModel.PatientReport:
                    Row("Patient Name", "Age", "Contact No.", "AOG", "EDC", "Date Registered");
                    foreach (var p in model.Patients)
                        Row(p.FullName, p.Age, p.ContactNo, p.AOG, p.EDC?.ToString("yyyy-MM-dd"), p.DateRegistered?.ToString("yyyy-MM-dd"));
                    break;
                case ReportsPageViewModel.AppointmentReport:
                    Row("Patient Name", "Date", "Time", "Service", "Status");
                    foreach (var a in model.Appointments)
                        Row(a.Patient?.FullName ?? a.PatientName, a.AppointmentDate.ToString("yyyy-MM-dd"), a.FormattedTime, a.ServiceType, a.Status);
                    break;
                case ReportsPageViewModel.ConsultationReport:
                    Row("Patient Name", "Date", "Visit Type", "Service", "Status");
                    foreach (var c in model.Consultations)
                        Row(c.Patient?.FullName, (c.CompletedAt ?? c.StartedAt).ToString("yyyy-MM-dd"), c.VisitType, c.ServiceType, c.Status);
                    break;
                case ReportsPageViewModel.PaymentReport:
                    Row("Patient Name", "Service", "Amount", "Payment Method", "Payment Date");
                    foreach (var p in model.Payments)
                        Row(p.Consultation?.Patient?.FullName, p.Consultation?.ServiceType, p.Amount, p.PaymentMethod, p.PaymentDate.ToString("yyyy-MM-dd HH:mm"));
                    Row("", "TOTAL", model.Payments.Sum(p => p.Amount ?? 0), "", "");
                    break;
                default:
                    TempData["ErrorMessage"] = "CSV export is available for the Patient, Appointment, Consultation and Payment reports.";
                    return RedirectToAction(nameof(Index), new { reportType, dateFrom, dateTo });
            }

            // UTF-8 BOM aron husto ang ₱ ug ñ inig abli sa Excel.
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"{model.ReportType}-Report-{DateTime.Now:yyyyMMdd-HHmm}.csv");
        }
        */
        // ==== [ST-REPORT-1] END ====
    }
}
