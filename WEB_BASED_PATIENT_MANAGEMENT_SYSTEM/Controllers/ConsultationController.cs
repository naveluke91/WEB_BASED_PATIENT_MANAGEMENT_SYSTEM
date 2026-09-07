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

        // Confirmed appointments without a started consultation appear as Waiting.
        // Started consultations, including walk-ins, are read from Consultations.
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
                    SortDate = appointment.AppointmentDate.Date.Add(appointment.AppointmentTime)
                };

                queue.Add(queueItem);
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

            // The reused service-form script reads these values to auto-fill the
            // patient information and activate the selected service form.
            ViewBag.Patients = model.Patients;
            ViewBag.Patient = model.OpenConsultation?.Patient;
            ViewBag.InitialPatientId = model.OpenConsultation?.PatientId;
            ViewBag.InitialPatientName = model.OpenConsultation?.Patient?.FullName;
            ViewBag.InitialService = model.OpenConsultation?.ServiceType;

            return View(model);
        }

        // Completed clinical forms belong to Consultation, not the Service
        // selection list. This action shows the exact record linked to the
        // completed consultation in read-only mode.
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
            return View(model);
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
                    model.ServiceForm.FamilyPlanningRecord = record;
            }
        }

        // Starts the confirmed appointment after a Yes/No confirmation.
        // The service comes from the patient's Service registration.  This is
        // the point at which the matching clinical record is created.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Begin(int patientId, int? appointmentId)
        {
            if (!appointmentId.HasValue)
            {
                TempData["ErrorMessage"] = "Walk-in consultation is not available yet.";
                return RedirectToAction(nameof(Index));
            }

            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Please select a registered patient.";
                return RedirectToAction(nameof(Index));
            }

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId.Value);
            if (appointment == null || appointment.Status != "Confirmed" || appointment.PatientId != patientId)
            {
                TempData["ErrorMessage"] = "Only a confirmed appointment for the selected patient can be started.";
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
            var serviceType = appointment.ServiceType?.Trim() ?? string.Empty;
            if (!AvailableServices.Contains(serviceType))
            {
                TempData["ErrorMessage"] = "Please select a service for this appointment before starting consultation.";
                return RedirectToAction(nameof(Index));
            }

            const string visitType = "Appointment";
            appointment.InProcess = true;
            appointment.ServiceType = serviceType;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var clinicalRecord = CreateClinicalRecord(patientId, serviceType);
                var consultation = new Consultation
                {
                    PatientId = patientId,
                    AppointmentId = appointmentId,
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

        // Saves the clinical form created when the consultation began.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveService(int consultationId, PrenatalRecord prenatalRecord,
            NewbornRecord newbornRecord, FamilyPlanningRecord familyPlanningRecord)
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

                consultation.Status = "Completed";
                consultation.CompletedAt = DateTime.Now;
                if (consultation.Appointment != null)
                    consultation.Appointment.InProcess = false;

                _context.SaveChanges();
                transaction.Commit();

                TempData["SuccessMessage"] = "Consultation and service record saved successfully.";
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The service record could not be saved. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Removes an unfinished consultation and its clinical record. Completed
        // records remain protected from deletion in the consultation queue.
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
