using System;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class ServiceRecordViewModel
    {
        public int RecordId { get; set; }
        public string RecordType { get; set; } // e.g. "Prenatal", "Newborn"
        public string PatientName { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public bool IsLocked { get; set; }
    }
}
