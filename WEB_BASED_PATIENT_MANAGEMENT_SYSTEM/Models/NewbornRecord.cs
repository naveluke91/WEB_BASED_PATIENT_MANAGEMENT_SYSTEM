using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    [Table("NewbornRecords")]
    public class NewbornRecord
    {
        public int Id { get; set; }

        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        // CLINICAL CHART FIELDS
        public string? CaseNo { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfAdmission { get; set; }

        public string? BabyName { get; set; }
        public string? Weight { get; set; }
        public string? Gender { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateTimeDelivered { get; set; }

        public string? BedNo { get; set; }
        public string? MotherName { get; set; }
        public string? MotherAddress { get; set; }

        public string? PlacentaOut { get; set; } // Complete or Incomplete
        public string? ApgarScore { get; set; }

        // Measurements
        public string? MeasurementHead { get; set; }
        public string? MeasurementChest { get; set; }
        public string? MeasurementAbdomen { get; set; }
        public string? MeasurementLength { get; set; }

        public string? Diagnosis { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateTimeOfDischarge { get; set; }

        // CONSENT FIELDS
        public string? ConsentClientName { get; set; }
        public int? ConsentClientAge { get; set; }
        public string? ConsentClientAddress { get; set; }
        public string? ConsentCivilStatus { get; set; }
        public string? ConsentRelationship { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? ConsentClientDate { get; set; }

        public string? ConsentMidwifeName { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? ConsentMidwifeDate { get; set; }

        // DYNAMIC TABLES (Mapped to JSON strings in DB)
        public string? VitalsJson { get; set; }
        public string? MedicationsJson { get; set; }

        [NotMapped]
        public List<NewbornVital> Vitals { get; set; } = new List<NewbornVital>();
        
        [NotMapped]
        public List<NewbornMedication> Medications { get; set; } = new List<NewbornMedication>();
    }

    // Helper classes for MVC Form Binding (These are not database tables, they are serialized to JSON in NewbornRecord)
    public class NewbornVital
    {
        public string? Index { get; set; } // Used for MVC binding

        [DataType(DataType.DateTime)]
        public DateTime? DateTime { get; set; }

        public string? HeartRate { get; set; }
        public string? RespiratoryRate { get; set; }
        public string? Temperature { get; set; }
    }

    public class NewbornMedication
    {
        public string? Index { get; set; } // Used for MVC binding

        [DataType(DataType.DateTime)]
        public DateTime? DateTime { get; set; }

        public string? MedicineGiven { get; set; }
    }
}
