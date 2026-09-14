namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class BillingPageViewModel
    {
        // Saved transactions, newest first (loaded with Consultation → Patient / Appointment).
        public List<Payment> Transactions { get; set; } = new();

        // Completed consultations that do not have a payment yet.
        public List<Consultation> BillableConsultations { get; set; } = new();

        // Registered service price per consultation ID; pre-fills Amount.
        public Dictionary<int, decimal> ServiceFees { get; set; } = new();

        public IReadOnlyList<string> PaymentMethods { get; set; } = Array.Empty<string>();

        // Opens Add Payment for this consultation (Consultation → Proceed to Billing).
        public int? OpenConsultationId { get; set; }

        // Opens this transaction's details (Consultation → View Payment).
        public int? OpenPaymentId { get; set; }
    }
}
    