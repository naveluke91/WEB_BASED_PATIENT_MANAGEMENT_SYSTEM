using System;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class ServiceFormViewModel
    {
        public string? RecordType { get; set; } // "Prenatal", "Newborn", "Family Planning"
        public bool IsViewOnly { get; set; }

        public PrenatalRecord? PrenatalRecord { get; set; }
        public NewbornRecord? NewbornRecord { get; set; }
        public FamilyPlanningRecord? FamilyPlanningRecord { get; set; }
    }
}
