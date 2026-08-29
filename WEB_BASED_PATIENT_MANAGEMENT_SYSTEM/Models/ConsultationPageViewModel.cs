namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class ConsultationPageViewModel
    {
        public List<ConsultationQueueItem> Queue { get; set; } = new();
        public List<Patient> Patients { get; set; } = new();
        public ServiceFormViewModel ServiceForm { get; set; } = new()
        {
            PrenatalRecord = new PrenatalRecord { RecordDate = DateTime.Today },
            NewbornRecord = new NewbornRecord(),
            FamilyPlanningRecord = new FamilyPlanningRecord()
        };
        public Consultation? OpenConsultation { get; set; }
    }

    public class ConsultationQueueItem
    {
        public int? ConsultationId { get; set; }
        public int? AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string VisitType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RecordType { get; set; }
        public int? RecordId { get; set; }
        public DateTime SortDate { get; set; }
    }
}
