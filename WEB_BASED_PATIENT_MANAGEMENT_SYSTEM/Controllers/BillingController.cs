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
            "GCash",

            // ==== [ST-BILLING-12] START — Bag-ong payment method: Maya ====
            /*
            "Maya",
            */
            // ==== [ST-BILLING-12] END ====
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
        // from the form.
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

            // Hangtod 2 ka decimal lang ang kantidad.
            if (payment.Amount.HasValue && decimal.Round(payment.Amount.Value, 2) != payment.Amount.Value)
            {
                ModelState.AddModelError(nameof(Payment.Amount), "Amount can only have up to 2 decimal places.");
            }

            // ==== [ST-BILLING-7] START — Amount validation: dili ₱0 ug dili ubos sa service fee ====
            // (Kung naay Senior/PWD discount, ang minimum kay 80% sa service fee.)
            /*
            var stFee = GetServiceFees(new List<Consultation> { consultation }).GetValueOrDefault(consultation.Id);
            var stMinimum = Request.Form["DiscountType"].ToString() is "Senior" or "PWD" ? Math.Round(stFee * 0.80m, 2) : stFee;
            if (payment.Amount.HasValue && payment.Amount <= 0)
                ModelState.AddModelError(nameof(Payment.Amount), "Amount must be more than ₱0.00.");
            else if (payment.Amount.HasValue && stFee > 0 && payment.Amount < stMinimum)
                ModelState.AddModelError(nameof(Payment.Amount), $"Amount cannot be less than ₱{stMinimum:N2}.");
            */
            // ==== [ST-BILLING-7] END ====

            // ==== [ST-BILLING-8] START — Cash: Amount Received (dili ubos sa Amount) ====
            /*
            if (paymentMethod == "Cash")
            {
                decimal? stTendered = decimal.TryParse(Request.Form["AmountTendered"].ToString(), System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var tendered) ? tendered : null;

                if (stTendered == null)
                    ModelState.AddModelError("AmountTendered", "Enter the amount received from the patient.");
                else if (payment.Amount.HasValue && stTendered < payment.Amount)
                    ModelState.AddModelError("AmountTendered", "Amount received cannot be less than the amount to pay.");
                else
                    payment.AmountTendered = stTendered;
            }
            */
            // ==== [ST-BILLING-8] END ====

            // ==== [ST-BILLING-9] START — GCash: Reference Number (13 digits, dili pwede magbalik) ====
            /*
            if (paymentMethod == "GCash")
            {
                var stReference = Request.Form["ReferenceNumber"].ToString().Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(stReference, @"^\d{13}$"))
                    ModelState.AddModelError("ReferenceNumber", "Enter the 13-digit GCash reference number.");
                else if (_context.Payments.Any(p => p.ReferenceNumber == stReference))
                    ModelState.AddModelError("ReferenceNumber", "This reference number was already used.");
                else
                    payment.ReferenceNumber = stReference;
            }
            */
            // ==== [ST-BILLING-9] END ====

            // ==== [ST-BILLING-10] START — Discount (Senior Citizen / PWD) ====
            /*
            var stDiscount = Request.Form["DiscountType"].ToString();
            payment.DiscountType = stDiscount is "Senior" or "PWD" ? stDiscount : null;
            */
            // ==== [ST-BILLING-10] END ====

            if (!ModelState.IsValid)
            {
                // Reopen Add Payment for the same consultation with the first error.
                TempData["AddPaymentError"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "Please check the payment details.";
                // Ang field nga may sayop (para ma-marka og pula).
                TempData["AddPaymentErrorField"] = ModelState.FirstOrDefault(entry => entry.Value?.Errors.Count > 0).Key;
                return RedirectToAction(nameof(Index), new { consultationId = consultation.Id });
            }

            payment.PaymentMethod = paymentMethod!;
            payment.PaymentDate = DateTime.Now;

            // ==== [ST-BILLING-11] START — Received By: ang naka-login nga staff ====
            /*
            payment.ReceivedBy = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? User.Identity?.Name;
            */
            // ==== [ST-BILLING-11] END ====

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

            // ==== [ST-USER-8] START — Audit trail: Add Payment ====
            /*
            StAudit.Log(_context, User.Identity?.Name, "Add Payment",
                $"Recorded ₱{payment.Amount:N2} ({payment.PaymentMethod}) for \"{consultation.Patient?.FullName}\".");
            */
            // ==== [ST-USER-8] END ====

            TempData["SuccessMessage"] = $"Payment for \"{consultation.Patient?.FullName}\" recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ==== [ST-BILLING-15] START — POST /Billing/Void/5: i-void (delete) ang payment (Admin ra) ====
        // Human ma-void, ang consultation mobalik sa "Unpaid" ug pwede bayran pag-usab.
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = UserRoles.Admin)]
        public IActionResult Void(int id)
        {
            var payment = _context.Payments
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                .FirstOrDefault(p => p.Id == id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Payment for \"{payment.Consultation?.Patient?.FullName}\" was voided.";
            return RedirectToAction(nameof(Index));
        }
        */
        // ==== [ST-BILLING-15] END ====

        // ==== [ST-BILLING-16] START — POST /Billing/EditPayment: usbon ang Amount ug Method (Admin ra) ====
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = UserRoles.Admin)]
        public IActionResult EditPayment(int id, decimal? amount, string? paymentMethod)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null) return NotFound();

            var method = PaymentMethods.FirstOrDefault(m => string.Equals(m, paymentMethod?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (method == null || amount is null or <= 0 || amount > 99999999.99m || decimal.Round(amount.Value, 2) != amount.Value)
            {
                TempData["ErrorMessage"] = "Please enter a valid amount and payment method.";
                return RedirectToAction(nameof(Index));
            }

            payment.Amount = amount;
            payment.PaymentMethod = method;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment was updated.";
            return RedirectToAction(nameof(Index));
        }
        */
        // ==== [ST-BILLING-16] END ====

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
