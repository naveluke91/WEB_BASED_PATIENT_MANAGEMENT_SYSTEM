using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// Payment/Billing — records the payment for a completed consultation and
    /// lists the saved transactions. There is no payment gateway; the module
    /// only records transactions.
    /// </summary>
    public class BillingController : Controller
    {
        // The only accepted payment methods. Create rejects any other submitted value.
        private static readonly string[] PaymentMethods =
        {
            "Cash",
            "GCash"
        };

        private const string DuplicatePaymentNotice =
            "A payment has already been recorded for this consultation, so no new transaction was created.";

        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------------
        // GET /Billing                   — View Transactions
        // GET /Billing?consultationId=5  — Add Payment for that consultation
        //                                  (Consultation → Proceed to Billing)
        // GET /Billing?paymentId=3       — opens that transaction's details
        // -----------------------------------------------------------------------
        public IActionResult Index(int? consultationId, int? paymentId)
        {
            if (consultationId.HasValue)
            {
                // Only the ID travels in the link. The consultation is read again
                // so its status and any existing payment are checked on the server.
                var consultation = _context.Consultations
                    .AsNoTracking()
                    .FirstOrDefault(c => c.Id == consultationId.Value);

                if (consultation == null || consultation.Status != "Completed")
                {
                    TempData["ErrorMessage"] = "Payment can only be recorded for a completed consultation.";
                    return RedirectToAction(nameof(Index));
                }

                var existingPayment = _context.Payments
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ConsultationId == consultation.Id);

                if (existingPayment != null)
                {
                    TempData["PaymentNotice"] = DuplicatePaymentNotice;
                    return RedirectToAction(nameof(Index), new { paymentId = existingPayment.Id });
                }
            }

            var model = new BillingPageViewModel
            {
                Transactions = _context.Payments
                    .AsNoTracking()
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Patient)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Appointment)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList(),

                // Only completed consultations without a payment can be billed.
                BillableConsultations = _context.Consultations
                    .AsNoTracking()
                    .Include(c => c.Patient)
                    .Include(c => c.Appointment)
                    .Where(c => c.Status == "Completed" && !_context.Payments.Any(p => p.ConsultationId == c.Id))
                    .OrderByDescending(c => c.CompletedAt)
                    .ToList(),

                PaymentMethods = PaymentMethods,
                OpenConsultationId = consultationId,
                OpenPaymentId = paymentId
            };

            model.ServiceFees = GetServiceFees(model.BillableConsultations);

            return View(model);
        }

        // -----------------------------------------------------------------------
        // POST /Billing/Create
        // Saves one payment for one completed consultation. Only the consultation
        // ID, amount and payment method are bound; patient details are never taken
        // from the form. Reference number and notes are no longer collected, so
        // they are not bound and stay empty.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind(nameof(Payment.ConsultationId), nameof(Payment.Amount),
            nameof(Payment.PaymentMethod))] Payment payment)
        {
            var consultation = _context.Consultations
                .Include(c => c.Patient)
                .FirstOrDefault(c => c.Id == payment.ConsultationId);

            // Server-side rule: a waiting or in-progress consultation cannot be
            // billed, even when the request does not come from the Add Payment modal.
            if (consultation == null || consultation.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Payment can only be recorded for a completed consultation.";
                return RedirectToAction(nameof(Index));
            }

            // One payment per consultation: open the saved transaction instead.
            var existingPayment = _context.Payments
                .AsNoTracking()
                .FirstOrDefault(p => p.ConsultationId == consultation.Id);

            if (existingPayment != null)
            {
                TempData["PaymentNotice"] = DuplicatePaymentNotice;
                return RedirectToAction(nameof(Index), new { paymentId = existingPayment.Id });
            }

            // Only Cash or GCash is accepted. Any other submitted value is rejected
            // here, not just left out of the dropdown.
            var paymentMethod = PaymentMethods.FirstOrDefault(method =>
                string.Equals(method, payment.PaymentMethod?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (paymentMethod == null)
            {
                ModelState.AddModelError(nameof(Payment.PaymentMethod), "Please select a valid payment method.");
            }

            if (!ModelState.IsValid)
            {
                // Reopen Add Payment for the same consultation with the first error.
                TempData["AddPaymentError"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "Please check the payment details.";
                return RedirectToAction(nameof(Index), new { consultationId = consultation.Id });
            }

            payment.PaymentMethod = paymentMethod!;
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // The unique ConsultationId index rejected a second payment saved at
                // the same moment (for example a double submit).
                _context.Entry(payment).State = EntityState.Detached;

                var savedPayment = _context.Payments
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ConsultationId == consultation.Id);

                if (savedPayment != null)
                {
                    TempData["PaymentNotice"] = DuplicatePaymentNotice;
                    return RedirectToAction(nameof(Index), new { paymentId = savedPayment.Id });
                }

                TempData["ErrorMessage"] = "The payment could not be saved. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Payment for \"{consultation.Patient?.FullName}\" recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // The suggested amount is the price saved on the Service record the
        // consultation came from: the walk-in Service (Consultation.ServiceId) or
        // the service registered for its appointment (Service.AppointmentId). The
        // service name must still match, so an edited service never supplies a
        // wrong price.
        private Dictionary<int, decimal> GetServiceFees(List<Consultation> consultations)
        {
            var serviceIds = consultations
                .Where(c => c.ServiceId.HasValue)
                .Select(c => c.ServiceId!.Value)
                .ToList();

            var appointmentIds = consultations
                .Where(c => c.AppointmentId.HasValue)
                .Select(c => c.AppointmentId!.Value)
                .ToList();

            var services = _context.Services
                .AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id)
                    || (s.AppointmentId.HasValue && appointmentIds.Contains(s.AppointmentId.Value)))
                .ToList();

            var fees = new Dictionary<int, decimal>();

            foreach (var consultation in consultations)
            {
                var service = consultation.ServiceId.HasValue
                    ? services.FirstOrDefault(s => s.Id == consultation.ServiceId.Value)
                    : services.FirstOrDefault(s => consultation.AppointmentId.HasValue
                        && s.AppointmentId == consultation.AppointmentId.Value);

                if (service != null && service.ServiceName == consultation.ServiceType)
                    fees[consultation.Id] = service.Price;
            }

            return fees;
        }
    }
}
