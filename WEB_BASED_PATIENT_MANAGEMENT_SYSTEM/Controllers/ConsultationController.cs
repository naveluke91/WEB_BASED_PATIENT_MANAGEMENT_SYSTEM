using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    public class ConsultationController : Controller
    {
        private static readonly string[] AvailableServices =
        {
            "Normal Delivery Fee & Newborn Care Package",
            "Prenatal",
            "Implant",
            "Implant Removal",
            "DEPO",
            "NORIFAM",
            "IUD Insertion",
            "IUD Removal",
            "Anti-Tetanus Injection"
        };

        private readonly ApplicationDbContext _context;

        public ConsultationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? openConsultationId)
        {
            var consultations = _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Appointment)
                .ToList();

            var queue = consultations
                .Select(c => new ConsultationQueueItem
                {
                    ConsultationId = c.Id,
                    AppointmentId = c.AppointmentId,
                    PatientId = c.PatientId,
                    PatientName = c.Patient?.FullName ?? c.Appointment?.PatientName ?? "Unknown",
                    ContactNo = c.Patient?.ContactNo ?? c.Appointment?.ContactNo ?? "â€”",
                    // Keep records created before VisitType was required readable.
                    // An encounter linked to an appointment is always an appointment;
                    // one without an appointment is a walk-in.
                    VisitType = string.IsNullOrWhiteSpace(c.VisitType)
                        ? (c.AppointmentId.HasValue ? "Appointment" : "Walk-In")
                        : c.VisitType,
                    ServiceType = string.IsNullOrWhiteSpace(c.ServiceType) ? "—" : c.ServiceType,
                    Status = c.Status,
                    RecordType = c.RecordType,
                    RecordId = c.RecordId,
                    SortDate = c.CompletedAt ?? c.StartedAt
                })
                .ToList();

            var paymentIdsByConsultation = _context.Payments
                .AsNoTracking()
                .Select(p => new { p.ConsultationId, p.Id })
                .ToDictionary(p => p.ConsultationId, p => p.Id);

            foreach (var item in queue.Where(i => i.ConsultationId.HasValue))
            {
                item.PaymentId = paymentIdsByConsultation.TryGetValue(item.ConsultationId!.Value, out var paymentId)
                    ? paymentId
                    : null;
            }

            var appointmentIdsWithConsultation = consultations
                .Where(c => c.AppointmentId.HasValue)
                .Select(c => c.AppointmentId!.Value)
                .ToHashSet();

            var waitingAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == "Confirmed" && a.PatientId.HasValue && !appointmentIdsWithConsultation.Contains(a.Id))
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            foreach (var appointment in waitingAppointments)
            {
                var queueItem = new ConsultationQueueItem
                {
                    AppointmentId = appointment.Id,
                    PatientId = appointment.PatientId!.Value,
                    PatientName = appointment.Patient?.FullName ?? appointment.PatientName,
                    ContactNo = appointment.Patient?.ContactNo ?? appointment.ContactNo,
                    VisitType = "Appointment",
                    ServiceType = string.IsNullOrWhiteSpace(appointment.ServiceType) ? "—" : appointment.ServiceType,
                    Status = "Waiting",
                    AppointmentDate = appointment.AppointmentDate,
                    SortDate = appointment.AppointmentDate.Date.Add(appointment.AppointmentTime)
                };

                queue.Add(queueItem);
            }

            var startedServiceIds = consultations
                .Where(c => c.ServiceId.HasValue)
                .Select(c => c.ServiceId!.Value)
                .ToHashSet();

            var waitingWalkInServices = _context.Services
                .Include(s => s.Patient)
                .Where(s => s.AppointmentId == null && !startedServiceIds.Contains(s.Id))
                .OrderBy(s => s.Id)
                .ToList();

            foreach (var service in waitingWalkInServices)
            {
                queue.Add(new ConsultationQueueItem
                {
                    ServiceId = service.Id,
                    PatientId = service.PatientId,
                    PatientName = service.Patient?.FullName ?? "Unknown",
                    ContactNo = service.Patient?.ContactNo ?? "—",
                    VisitType = "Walk-In",
                    ServiceType = service.ServiceName,
                    Status = "Waiting",
                    SortDate = DateTime.MinValue.AddSeconds(service.Id)
                });
            }

            var model = new ConsultationPageViewModel
            {
                Queue = queue
                    .OrderBy(item => item.Status == "In Progress" ? 0 : item.Status == "Waiting" ? 1 : 2)
                    .ThenByDescending(item => item.SortDate)
                    .ToList(),
                Patients = _context.Patients.OrderBy(p => p.FullName).ToList()
            };

            if (openConsultationId.HasValue)
            {
                model.OpenConsultation = _context.Consultations
                    .Include(c => c.Patient)
                    .FirstOrDefault(c => c.Id == openConsultationId.Value && c.Status == "In Progress");

                if (model.OpenConsultation != null)
                    LoadRegisteredServiceForm(model);
            }

            ViewBag.Patients = model.Patients;
            ViewBag.Patient = model.OpenConsultation?.Patient;
            ViewBag.InitialPatientId = model.OpenConsultation?.PatientId;
            ViewBag.InitialPatientName = model.OpenConsultation?.Patient?.FullName;
            ViewBag.InitialService = model.OpenConsultation?.ServiceType;
            ViewBag.ClinicalRules = ClinicalRules;

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewRecord(int id)
        {
            var consultation = _context.Consultations
                .Include(c => c.Patient)
                .FirstOrDefault(c => c.Id == id && c.Status == "Completed");

            if (consultation == null)
            {
                return NotFound();
            }

            var model = new ConsultationPageViewModel
            {
                OpenConsultation = consultation,
                Patients = _context.Patients.OrderBy(p => p.FullName).ToList()
            };

            LoadRegisteredServiceForm(model);
            return PartialView(model);
        }

        private void LoadRegisteredServiceForm(ConsultationPageViewModel model)
        {
            var consultation = model.OpenConsultation!;

            if (consultation.RecordType == "Prenatal" && consultation.RecordId.HasValue)
            {
                var record = _context.PrenatalRecords
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Id == consultation.RecordId.Value
                        && r.PatientId == consultation.PatientId);
                if (record != null)
                {
                    if (!string.IsNullOrWhiteSpace(record.PrenatalVisitsJson))
                    {
                        record.PrenatalVisits = System.Text.Json.JsonSerializer
                            .Deserialize<List<PrenatalVisit>>(record.PrenatalVisitsJson) ?? new List<PrenatalVisit>();
                    }
                    model.ServiceForm.PrenatalRecord = record;
                }
            }
            else if (consultation.RecordType == "Newborn" && consultation.RecordId.HasValue)
            {
                var record = _context.NewbornRecords
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Id == consultation.RecordId.Value
                        && r.PatientId == consultation.PatientId);
                if (record != null)
                {
                    if (!string.IsNullOrWhiteSpace(record.VitalsJson))
                    {
                        record.Vitals = System.Text.Json.JsonSerializer
                            .Deserialize<List<NewbornVital>>(record.VitalsJson) ?? new List<NewbornVital>();
                    }
                    if (!string.IsNullOrWhiteSpace(record.MedicationsJson))
                    {
                        record.Medications = System.Text.Json.JsonSerializer
                            .Deserialize<List<NewbornMedication>>(record.MedicationsJson) ?? new List<NewbornMedication>();
                    }
                    model.ServiceForm.NewbornRecord = record;
                }
            }
            else if (consultation.RecordType == "Family Planning" && consultation.RecordId.HasValue)
            {
                var record = _context.FamilyPlanningRecords
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Id == consultation.RecordId.Value
                        && r.PatientId == consultation.PatientId);
                if (record != null)
                {
                    record.Ack_MethodAccepted = consultation.ServiceType;
                    model.ServiceForm.FamilyPlanningRecord = record;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Begin(int patientId, int? appointmentId, int? serviceId)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Please select a registered patient.";
                return RedirectToAction(nameof(Index));
            }

            if (appointmentId.HasValue)
            {
                var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId.Value);
                if (appointment == null || appointment.Status != "Confirmed" || appointment.PatientId != patientId)
                {
                    TempData["ErrorMessage"] = "Only a confirmed appointment for the selected patient can be started.";
                    return RedirectToAction(nameof(Index));
                }

                if (appointment.AppointmentDate.Date > DateTime.Today)
                {
                    TempData["ErrorMessage"] = $"This appointment is on {appointment.AppointmentDate:MMMM dd, yyyy}. It can only be started on that day.";
                    return RedirectToAction(nameof(Index));
                }

                var existingConsultation = _context.Consultations.FirstOrDefault(c => c.AppointmentId == appointmentId.Value);
                if (existingConsultation != null)
                {
                    return RedirectToAction(nameof(Index), new { openConsultationId = existingConsultation.Id });
                }

                // The service belongs to this appointment, not to every appointment
                // of the same patient. A new appointment must never inherit a prior
                // service selected for that patient.
                var serviceType = _context.Services
                    .Where(s => s.AppointmentId == appointment.Id && s.PatientId == patientId)
                    .Select(s => s.ServiceName)
                    .FirstOrDefault() ?? string.Empty;
                if (!AvailableServices.Contains(serviceType))
                {
                    TempData["ErrorMessage"] = "Please select a service for this appointment before starting consultation.";
                    return RedirectToAction(nameof(Index));
                }

                appointment.InProcess = true;
                appointment.ServiceType = serviceType;

                return StartConsultation(patientId, "Appointment", serviceType, appointmentId, null);
            }

            if (!serviceId.HasValue)
            {
                TempData["ErrorMessage"] = "Please select a registered service.";
                return RedirectToAction(nameof(Index));
            }

            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId.Value && s.PatientId == patientId && s.AppointmentId == null);
            if (service == null)
            {
                TempData["ErrorMessage"] = "That service is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            var existingServiceConsultation = _context.Consultations.FirstOrDefault(c => c.ServiceId == serviceId.Value);
            if (existingServiceConsultation != null)
            {
                return RedirectToAction(nameof(Index), new { openConsultationId = existingServiceConsultation.Id });
            }

            if (!AvailableServices.Contains(service.ServiceName))
            {
                TempData["ErrorMessage"] = "The selected service is not supported.";
                return RedirectToAction(nameof(Index));
            }

            return StartConsultation(patientId, "Walk-In", service.ServiceName, null, serviceId);
        }

        private IActionResult StartConsultation(int patientId, string visitType, string serviceType, int? appointmentId, int? serviceId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var clinicalRecord = CreateClinicalRecord(patientId, serviceType);
                var consultation = new Consultation
                {
                    PatientId = patientId,
                    AppointmentId = appointmentId,
                    ServiceId = serviceId,
                    VisitType = visitType,
                    ServiceType = serviceType,
                    Status = "In Progress",
                    StartedAt = DateTime.Now,
                    RecordType = clinicalRecord.RecordType,
                    RecordId = clinicalRecord.RecordId
                };

                _context.Consultations.Add(consultation);
                _context.SaveChanges();
                transaction.Commit();

                return RedirectToAction(nameof(Index), new { openConsultationId = consultation.Id });
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The consultation could not be started. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private (string RecordType, int RecordId) CreateClinicalRecord(int patientId, string serviceName)
        {
            if (serviceName == "Prenatal")
            {
                var record = new PrenatalRecord
                {
                    PatientId = patientId,
                    RecordDate = DateTime.Today,
                    SelectedServices = serviceName
                };
                _context.PrenatalRecords.Add(record);
                _context.SaveChanges();
                return ("Prenatal", record.Id);
            }

            if (serviceName == "Normal Delivery Fee & Newborn Care Package")
            {
                var record = new NewbornRecord
                {
                    PatientId = patientId
                };
                _context.NewbornRecords.Add(record);
                _context.SaveChanges();
                return ("Newborn", record.Id);
            }

            if (AvailableServices.Contains(serviceName))
            {
                var record = new FamilyPlanningRecord
                {
                    PatientId = patientId,
                    RecordDate = DateTime.Today,
                    SelectedService = serviceName
                };
                _context.FamilyPlanningRecords.Add(record);
                _context.SaveChanges();
                return ("Family Planning", record.Id);
            }

            throw new InvalidOperationException("The selected service is not supported.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveService(int consultationId, PrenatalRecord prenatalRecord,
            NewbornRecord newbornRecord, FamilyPlanningRecord familyPlanningRecord, bool saveLater = false)
        {
            var consultation = _context.Consultations
                .Include(c => c.Appointment)
                .FirstOrDefault(c => c.Id == consultationId);

            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consultation not found.";
                return RedirectToAction(nameof(Index));
            }

            if (consultation.Status == "Completed")
            {
                TempData["ErrorMessage"] = "This consultation is already completed.";
                return RedirectToAction(nameof(Index));
            }

            var (recordPrefix, record) = consultation.ServiceType switch
            {
                "Prenatal" => ("PrenatalRecord", (object)prenatalRecord),
                "Normal Delivery Fee & Newborn Care Package" => ("NewbornRecord", newbornRecord),
                _ => ("FamilyPlanningRecord", familyPlanningRecord)
            };
            ApplyMultiValueFields(record, recordPrefix);
            familyPlanningRecord.Ack_MethodAccepted = consultation.ServiceType;

            var clinicalErrors = ValidateClinicalRecord(record, recordPrefix, draft: saveLater);
            if (clinicalErrors.Count > 0)
            {
                TempData["ErrorMessage"] = "Unable to save the record. " + clinicalErrors.Values.First();
                TempData["ClinicalErrors"] = System.Text.Json.JsonSerializer.Serialize(clinicalErrors);
                return RedirectToAction(nameof(Index), new { openConsultationId = consultation.Id });
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (consultation.ServiceType == "Prenatal")
                {
                    var existingRecord = consultation.RecordId.HasValue
                        ? _context.PrenatalRecords.FirstOrDefault(r => r.Id == consultation.RecordId.Value
                            && r.PatientId == consultation.PatientId)
                        : null;
                    if (existingRecord == null)
                        throw new InvalidOperationException("Consultation prenatal record not found.");

                    if (prenatalRecord.PrenatalVisits != null && prenatalRecord.PrenatalVisits.Count > 0)
                        prenatalRecord.PrenatalVisitsJson = System.Text.Json.JsonSerializer.Serialize(prenatalRecord.PrenatalVisits);

                    prenatalRecord.Id = existingRecord.Id;
                    prenatalRecord.PatientId = consultation.PatientId;
                    prenatalRecord.RecordDate ??= DateTime.Today;
                    prenatalRecord.SelectedServices = consultation.ServiceType;
                    _context.Entry(existingRecord).CurrentValues.SetValues(prenatalRecord);
                    _context.SaveChanges();

                    consultation.RecordType = "Prenatal";
                    consultation.RecordId = existingRecord.Id;
                }
                else if (consultation.ServiceType == "Normal Delivery Fee & Newborn Care Package")
                {
                    var existingRecord = consultation.RecordId.HasValue
                        ? _context.NewbornRecords.FirstOrDefault(r => r.Id == consultation.RecordId.Value
                            && r.PatientId == consultation.PatientId)
                        : null;
                    if (existingRecord == null)
                        throw new InvalidOperationException("Consultation newborn record not found.");

                    if (newbornRecord.Vitals != null && newbornRecord.Vitals.Count > 0)
                        newbornRecord.VitalsJson = System.Text.Json.JsonSerializer.Serialize(newbornRecord.Vitals);
                    if (newbornRecord.Medications != null && newbornRecord.Medications.Count > 0)
                        newbornRecord.MedicationsJson = System.Text.Json.JsonSerializer.Serialize(newbornRecord.Medications);

                    newbornRecord.Id = existingRecord.Id;
                    newbornRecord.PatientId = consultation.PatientId;
                    _context.Entry(existingRecord).CurrentValues.SetValues(newbornRecord);
                    _context.SaveChanges();

                    consultation.RecordType = "Newborn";
                    consultation.RecordId = existingRecord.Id;
                }
                else if (AvailableServices.Contains(consultation.ServiceType))
                {
                    var existingRecord = consultation.RecordId.HasValue
                        ? _context.FamilyPlanningRecords.FirstOrDefault(r => r.Id == consultation.RecordId.Value
                            && r.PatientId == consultation.PatientId)
                        : null;
                    if (existingRecord == null)
                        throw new InvalidOperationException("Consultation family-planning record not found.");

                    familyPlanningRecord.Id = existingRecord.Id;
                    familyPlanningRecord.PatientId = consultation.PatientId;
                    familyPlanningRecord.RecordDate ??= DateTime.Today;
                    familyPlanningRecord.SelectedService = consultation.ServiceType;
                    _context.Entry(existingRecord).CurrentValues.SetValues(familyPlanningRecord);
                    _context.SaveChanges();

                    consultation.RecordType = "Family Planning";
                    consultation.RecordId = existingRecord.Id;
                }
                else
                {
                    TempData["ErrorMessage"] = "The selected service is not supported.";
                    return RedirectToAction(nameof(Index));
                }

                if (!saveLater)
                {
                    consultation.Status = "Completed";
                    consultation.CompletedAt = DateTime.Now;
                    if (consultation.Appointment != null)
                        consultation.Appointment.InProcess = false;
                }

                _context.SaveChanges();
                transaction.Commit();

                TempData["SuccessMessage"] = saveLater
                    ? "Consultation saved. You can continue it later."
                    : "Consultation and service record saved successfully.";
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The service record could not be saved. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var consultation = _context.Consultations
                .Include(c => c.Appointment)
                .FirstOrDefault(c => c.Id == id);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "In Progress")
            {
                TempData["ErrorMessage"] = "Only an in-progress consultation can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                RemoveLinkedClinicalRecord(consultation);

                if (consultation.Appointment != null)
                    consultation.Appointment.InProcess = false;

                _context.Consultations.Remove(consultation);
                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The consultation could not be deleted. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        public sealed record ClinicalFieldRule(string Field, string Type, decimal? Min = null, decimal? Max = null,
            bool NotFuture = false, string? After = null, string[]? Allowed = null, string? Pattern = null, string? Message = null,
            bool Required = false);

        private const string TfalRulePattern = @"^\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}$";
        private const string AogRulePattern = @"^(\d{1,2})(\.\d{1,2})?\s*(weeks?|wks?|w)?(\s*(and\s+)?[0-6]\s*(days?|d))?$";
        private const string BloodPressurePattern = @"^\d{2,3}\s*/\s*\d{2,3}(\s*mmhg)?$";

        private static readonly List<ClinicalFieldRule> ClinicalRules = BuildClinicalRules();

        // Mga checkbox group: i-save tanan nga gi-check, dili lang ang una.
        private static readonly Dictionary<string, string[]> CheckboxGroups = new()
        {
            ["PrenatalRecord"] = new[] { "B2_QuickCheck_Checkboxes", "B3_RAM_Checkboxes", "PrioritySigns_Checkboxes", "ThirdTrimester_Checkboxes", "PatientProblems_Checkboxes" },
            ["FamilyPlanningRecord"] = new[] { "ReasonForFP", "ReasonChanging", "MethodCurrentlyUsed", "VAW_ReferredTo", "PE_Skin", "PE_Conjunctiva", "PE_Neck", "PE_Breast", "PE_Abdomen", "PE_Extremities", "Pelvic_CervicalAbnormalities" }
        };

        // Mga field nga duha ka beses makita sa Newborn form.
        private static readonly string[] NewbornRepeatedFields = { "BabyName", "BedNo", "ConsentClientName" };

        private static List<ClinicalFieldRule> BuildClinicalRules()
        {
            var rules = new List<ClinicalFieldRule>();
            void Whole(string field, decimal min, decimal max, string? message = null) => rules.Add(new(field, "whole", min, max, Message: message));
            void Number(string field, decimal min, decimal max) => rules.Add(new(field, "decimal", min, max));
            void Date(string field, bool notFuture = false, string? after = null, string? message = null) =>
                rules.Add(new(field, "date", NotFuture: notFuture, After: after, Message: message));
            void Choice(string field, params string[] allowed) => rules.Add(new(field, "choice", Allowed: allowed));
            void Pattern(string field, string pattern, string message) => rules.Add(new(field, "pattern", Pattern: pattern, Message: message));
            string[] yesNo = { "yes", "no" };

            foreach (var field in new[] { "Gravida", "G", "T", "P", "A", "L" }) Whole("PrenatalRecord." + field, 0, 99);
            Whole("PrenatalRecord.Menarche", 5, 30, "Enter a valid age at menarche.");
            Number("PrenatalRecord.Weight", 1, 300);
            Number("PrenatalRecord.Temperature", 30, 45);
            Pattern("PrenatalRecord.TFAL", TfalRulePattern, "Use the format 2-1-0-1.");
            Pattern("PrenatalRecord.AOG", AogRulePattern, "Enter a valid AOG (example: 14 weeks).");
            Pattern("PrenatalRecord.BloodPressure", BloodPressurePattern, "Use the format 120/80.");
            Pattern("PrenatalRecord.C3_PreEclampsia_BP", BloodPressurePattern, "Use the format 120/80.");
            Pattern("PrenatalRecord.PrenatalVisits[].AOG", AogRulePattern, "Enter a valid AOG (example: 14 weeks).");
            Number("PrenatalRecord.PrenatalVisits[].Weight", 1, 300);
            Pattern("PrenatalRecord.PrenatalVisits[].BloodPressure", BloodPressurePattern, "Use the format 120/80.");
            Number("PrenatalRecord.PrenatalVisits[].Temperature", 30, 45);
            Date("PrenatalRecord.RecordDate");
            Date("PrenatalRecord.AntenatalDate");
            Date("PrenatalRecord.DateOfBirth", notFuture: true);
            Date("PrenatalRecord.LMP", notFuture: true);
            Date("PrenatalRecord.C2_LMP", notFuture: true);
            Date("PrenatalRecord.EDC", after: "LMP", message: "Date must be after the LMP.");
            Date("PrenatalRecord.C2_EDC", after: "C2_LMP", message: "Date must be after the LMP.");
            Choice("PrenatalRecord.VisitType", "initial", "followup");
            Choice("PrenatalRecord.PrioritySigns_YesNo", yesNo);
            Choice("PrenatalRecord.PatientProblems_YesNo", yesNo);

            Whole("NewbornRecord.ConsentClientAge", 0, 130);
            Date("NewbornRecord.DateTimeOfAdmission", notFuture: true);
            Date("NewbornRecord.DateTimeDelivered", notFuture: true);
            Date("NewbornRecord.DateTimeOfDischarge");
            Date("NewbornRecord.ConsentClientDate");
            Date("NewbornRecord.ConsentMidwifeDate");
            Choice("NewbornRecord.PlacentaOut", "complete", "incomplete");
            Whole("NewbornRecord.Vitals[].HeartRate", 40, 250);
            Whole("NewbornRecord.Vitals[].RespiratoryRate", 10, 120);
            Number("NewbornRecord.Vitals[].Temperature", 30, 45);

            Whole("FamilyPlanningRecord.ClientAge", 0, 130);
            Whole("FamilyPlanningRecord.SpouseAge", 0, 130);
            foreach (var field in new[] { "NoOfLivingChildren", "OH_Gravida", "OH_Para", "OH_Abortion", "OH_LivingChildren" })
                Whole("FamilyPlanningRecord." + field, 0, 99);
            Number("FamilyPlanningRecord.PE_Height", 0.3m, 2.5m);
            Number("FamilyPlanningRecord.PE_Weight", 1, 300);
            Whole("FamilyPlanningRecord.PE_BloodPressure_Systolic", 40, 300);
            Whole("FamilyPlanningRecord.PE_BloodPressure_Diastolic", 20, 200);
            Whole("FamilyPlanningRecord.PE_PulseRate", 20, 250);
            Number("FamilyPlanningRecord.Pelvic_UterineDepth", 0, 20);
            foreach (var field in new[] { "ClientDateOfBirth", "SpouseDateOfBirth", "OH_DateOfLastDelivery", "OH_LastMenstrualPeriod", "OH_PreviousMenstrualPeriod" })
                Date("FamilyPlanningRecord." + field, notFuture: true);
            foreach (var field in new[] { "RecordDate", "Ack_ClientDate", "Ack_ParentDate" })
                Date("FamilyPlanningRecord." + field);
            Choice("FamilyPlanningRecord.CivilStatus", "Single", "Married", "Widowed", "Separated", "Live-in");
            Choice("FamilyPlanningRecord.PlanMoreChildren", yesNo);
            Choice("FamilyPlanningRecord.TypeOfClient", "new", "current", "changing", "clinic", "dropout");
            foreach (var field in new[]
            {
                "MH_SevereHeadaches", "MH_StrokeHeartHypertension", "MH_HematomaBruising", "MH_BreastCancerMass",
                "MH_SevereChestPain", "MH_Cough14Days", "MH_Jaundice", "MH_UnexplainedVaginalBleeding",
                "MH_AbnormalVaginalDischarge", "MH_PhenobarbitalRifampicin", "MH_Smoker", "MH_WithDisability",
                "STI_AbnormalDischarge", "STI_SoresUlcers", "STI_PainBurning", "STI_HistoryTreatment", "STI_HIV_PID",
                "VAW_UnpleasantRelationship", "VAW_PartnerDisapprove", "VAW_HistoryDomesticViolence"
            })
                Choice("FamilyPlanningRecord." + field, yesNo);
            Choice("FamilyPlanningRecord.OH_TypeOfLastDelivery", "vaginal", "cs");
            Choice("FamilyPlanningRecord.OH_MenstrualFlow", "scanty", "moderate", "heavy");
            Choice("FamilyPlanningRecord.STI_AbnormalDischarge_Loc", "vagina", "penis");
            Choice("FamilyPlanningRecord.Pelvic_CervicalConsistency", "firm", "soft");
            Choice("FamilyPlanningRecord.Pelvic_UterinePosition", "mid", "ante", "retro");
            foreach (var field in new[]
            {
                "OH_FullTerm", "OH_Premature", "OH_Dysmenorrhea", "OH_HydatidiformMole", "OH_EctopicPregnancy",
                "Pelvic_Normal", "Pelvic_Mass", "Pelvic_AbnormalDischarge", "Pelvic_CervicalTenderness", "Pelvic_AdnexalMassTenderness"
            })
                Choice("FamilyPlanningRecord." + field, "yes");

            foreach (var field in new[]
            {
                "PrenatalRecord.Weight", "PrenatalRecord.Temperature", "PrenatalRecord.BloodPressure",
                "NewbornRecord.Weight", "NewbornRecord.Gender", "NewbornRecord.DateTimeDelivered",
                "FamilyPlanningRecord.PE_Weight", "FamilyPlanningRecord.PE_BloodPressure_Systolic", "FamilyPlanningRecord.PE_BloodPressure_Diastolic"
            })
            {
                var index = rules.FindIndex(rule => rule.Field == field);
                if (index >= 0)
                    rules[index] = rules[index] with { Required = true };
                else
                    rules.Add(new(field, "text", Required: true));
            }

            return rules;
        }

        private void ApplyMultiValueFields(object record, string prefix)
        {
            var type = record.GetType();

            if (CheckboxGroups.TryGetValue(prefix, out var groups))
            {
                foreach (var name in groups)
                {
                    var values = Request.Form[$"{prefix}.{name}"].Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                    type.GetProperty(name)?.SetValue(record, values.Length > 0 ? string.Join(",", values) : null);
                }
            }

            if (prefix == "NewbornRecord")
            {
                foreach (var name in NewbornRepeatedFields)
                {
                    var value = Request.Form[$"{prefix}.{name}"].FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                    type.GetProperty(name)?.SetValue(record, value?.Trim());
                }
            }
        }

        private Dictionary<string, string> ValidateClinicalRecord(object record, string prefix, bool draft = false)
        {
            var errors = new Dictionary<string, string>();

            foreach (var entry in ModelState.Where(e => e.Value?.Errors.Count > 0
                && e.Key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)))
            {
                errors.TryAdd(prefix + entry.Key[prefix.Length..], "Enter a valid value.");
            }

            foreach (var clinicalRule in ClinicalRules.Where(r => r.Field.StartsWith(prefix + ".")))
            {
                var rule = draft ? clinicalRule with { Required = false } : clinicalRule;
                if (errors.ContainsKey(rule.Field))
                    continue;

                var rowField = rule.Field[(prefix.Length + 1)..].Split("[].");
                if (rowField.Length == 2)
                {
                    var rows = record.GetType().GetProperty(rowField[0])?.GetValue(record) as System.Collections.IEnumerable;
                    foreach (var row in rows ?? Array.Empty<object>())
                    {
                        var rowMessage = CheckClinicalRule(rule, row.GetType().GetProperty(rowField[1])?.GetValue(row), row);
                        if (rowMessage != null)
                        {
                            errors.TryAdd(prefix + ".Rows", rowMessage);
                            break;
                        }
                    }
                    continue;
                }

                var property = record.GetType().GetProperty(rule.Field[(prefix.Length + 1)..]);
                var message = property == null ? null : CheckClinicalRule(rule, property.GetValue(record), record);
                if (message != null)
                    errors[rule.Field] = message;
            }

            var rowDates = record switch
            {
                PrenatalRecord prenatal => prenatal.PrenatalVisits.Select(v => v.RecordDate),
                NewbornRecord newborn => newborn.Vitals.Select(v => v.DateTime).Concat(newborn.Medications.Select(m => m.DateTime)),
                _ => Enumerable.Empty<DateTime?>()
            };
            if (rowDates.Any(d => d.HasValue && (d.Value.Year < 1900 || d.Value.Year > 2100)))
                errors.TryAdd(prefix + ".Rows", "Enter a valid date in the table.");

            return errors;
        }

        private static string? CheckClinicalRule(ClinicalFieldRule rule, object? value, object record)
        {
            if (value == null || (value is string blank && string.IsNullOrWhiteSpace(blank)))
                return rule.Required ? "This field is required." : null;

            var text = (value as string)?.Trim() ?? string.Empty;
            var range = $"Enter a value from {rule.Min?.ToString("0.##", CultureInfo.InvariantCulture)} to {rule.Max?.ToString("0.##", CultureInfo.InvariantCulture)}.";

            switch (rule.Type)
            {
                case "whole":
                {
                    decimal number;
                    if (value is int whole)
                        number = whole;
                    else if (!Regex.IsMatch(text, @"^\d{1,4}$") || !decimal.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out number))
                        return rule.Message ?? "Enter a whole number (no decimals or negative numbers).";
                    return number < rule.Min || number > rule.Max ? rule.Message ?? range : null;
                }
                case "decimal":
                {
                    if (!Regex.IsMatch(text, @"^\d{1,4}(\.\d{1,2})?$")
                        || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                        return "Enter a number (example: 36.5).";
                    return number < rule.Min || number > rule.Max ? range : null;
                }
                case "pattern":
                    return Regex.IsMatch(text, rule.Pattern!, RegexOptions.IgnoreCase) ? null : rule.Message;
                case "choice":
                    return rule.Allowed!.Contains(text) ? null : "Please select a valid option.";
                case "date":
                {
                    var date = (DateTime)value;
                    if (date.Year < 1900 || date.Year > 2100)
                        return "Enter a valid date.";
                    if (rule.NotFuture && date.Date > DateTime.Today)
                        return "Date cannot be in the future.";
                    if (rule.After != null && record.GetType().GetProperty(rule.After)?.GetValue(record) is DateTime other
                        && date.Date <= other.Date)
                        return rule.Message;
                    return null;
                }
                default:
                    return null;
            }
        }

        private void RemoveLinkedClinicalRecord(Consultation consultation)
        {
            if (!consultation.RecordId.HasValue)
                return;

            if (consultation.RecordType == "Prenatal")
            {
                var record = _context.PrenatalRecords.FirstOrDefault(r =>
                    r.Id == consultation.RecordId.Value && r.PatientId == consultation.PatientId);
                if (record != null)
                    _context.PrenatalRecords.Remove(record);
            }
            else if (consultation.RecordType == "Newborn")
            {
                var record = _context.NewbornRecords.FirstOrDefault(r =>
                    r.Id == consultation.RecordId.Value && r.PatientId == consultation.PatientId);
                if (record != null)
                    _context.NewbornRecords.Remove(record);
            }
            else if (consultation.RecordType == "Family Planning")
            {
                var record = _context.FamilyPlanningRecords.FirstOrDefault(r =>
                    r.Id == consultation.RecordId.Value && r.PatientId == consultation.PatientId);
                if (record != null)
                    _context.FamilyPlanningRecords.Remove(record);
            }
        }
    }
}
