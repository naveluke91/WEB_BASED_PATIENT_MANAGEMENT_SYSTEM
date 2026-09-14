namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Reports → Generate Reports: the filters and the rows of the generated report.
    /// The rows are the existing records; nothing is copied or stored for reports.
    /// </summary>
    public class ReportsPageViewModel
    {
        public const string PatientReport = "Patient";
        public const string AppointmentReport = "Appointment";
        public const string ConsultationReport = "Consultation";
        public const string PaymentReport = "Payment";

        public static readonly (string Value, string Label)[] ReportTypes =
        {
            (PatientReport, "Patient Report"),
            (AppointmentReport, "Appointment Report"),
            (ConsultationReport, "Consultation Report"),
            (PaymentReport, "Payment/Billing Report")
        };

        public string? ReportType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Set once a valid report has been generated.
        public bool IsGenerated { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string? ErrorMessage { get; set; }

        public List<Patient> Patients { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
        public List<Consultation> Consultations { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }
}
