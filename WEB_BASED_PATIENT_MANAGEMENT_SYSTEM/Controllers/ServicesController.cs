using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    public class ServicesController : Controller
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

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- INDEX ---
        public IActionResult Index()
        {
            var viewModels = new List<ServiceRecordViewModel>();
            ViewBag.Patients = _context.Patients.OrderBy(p => p.FullName).ToList();

            var prenatalRecords = _context.PrenatalRecords.Include(r => r.Patient).ToList();
            foreach(var pr in prenatalRecords)
            {
                viewModels.Add(new ServiceRecordViewModel
                {
                    RecordId = pr.Id,
                    RecordType = "Prenatal",
                    PatientName = pr.Patient?.FullName ?? (!string.IsNullOrEmpty(pr.PatientName) ? pr.PatientName : "Unknown"),
                    ServiceName = pr.SelectedServices ?? "Prenatal",
                    Date = pr.RecordDate ?? DateTime.Now
                });
            }

            var newbornRecords = _context.NewbornRecords.Include(r => r.Patient).ToList();
            foreach(var nb in newbornRecords)
            {
                viewModels.Add(new ServiceRecordViewModel
                {
                    RecordId = nb.Id,
                    RecordType = "Newborn",
                    PatientName = nb.Patient?.FullName ?? (!string.IsNullOrEmpty(nb.MotherName) ? nb.MotherName : (nb.BabyName ?? "Unknown")),
                    ServiceName = "Normal Delivery Fee & Newborn Care Package",
                    Date = nb.DateTimeOfAdmission ?? DateTime.Now
                });
            }

            var fpRecords = _context.FamilyPlanningRecords.Include(r => r.Patient).ToList();
            foreach(var fp in fpRecords)
            {
                viewModels.Add(new ServiceRecordViewModel
                {
                    RecordId = fp.Id,
                    RecordType = "Family Planning",
                    PatientName = fp.Patient?.FullName ?? (!string.IsNullOrEmpty(fp.ClientLastName) ? $"{fp.ClientGivenName} {fp.ClientLastName}".Trim() : "Unknown"),
                    ServiceName = fp.SelectedService ?? "Family Planning",
                    Date = fp.RecordDate ?? DateTime.Now
                });
            }

            var linkedRecords = _context.Consultations
                .AsNoTracking()
                .Where(c => c.Status == "Completed"
                    && c.RecordId.HasValue
                    && !string.IsNullOrWhiteSpace(c.RecordType))
                .Select(c => new { c.RecordType, RecordId = c.RecordId!.Value })
                .ToList();

            foreach (var record in viewModels)
            {
                record.IsLocked = linkedRecords.Any(link =>
                    link.RecordId == record.RecordId && link.RecordType == record.RecordType);
            }

            var sortedList = viewModels.OrderByDescending(v => v.Date).ToList();
            return View(sortedList);
        }

        // --- CREATE (GET) ---
        [HttpGet]
        public IActionResult Create(int? patientId)
        {
            var availablePatients = _context.Patients.OrderBy(p => p.FullName).ToList();
            ViewBag.Patients = availablePatients;
            if (patientId.HasValue)
            {
                ViewBag.InitialPatientId = patientId.Value;
                var p = _context.Patients.FirstOrDefault(x => x.Id == patientId.Value);
                if (p != null) ViewBag.InitialPatientName = p.FullName;
            }
            return View(new ServiceFormViewModel { PrenatalRecord = new PrenatalRecord { RecordDate = DateTime.Today }, NewbornRecord = new NewbornRecord(), FamilyPlanningRecord = new FamilyPlanningRecord() });
        }

        // --- CREATE (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string? SelectedServices, int? SelectedPatientId)
        {
            if (!SelectedPatientId.HasValue
                || !_context.Patients.Any(p => p.Id == SelectedPatientId.Value))
            {
                TempData["ErrorMessage"] = "Please select a registered patient.";
                return RedirectToAction(nameof(Create));
            }

            if (string.IsNullOrWhiteSpace(SelectedServices)
                || !AvailableServices.Contains(SelectedServices))
            {
                TempData["ErrorMessage"] = "Please select a valid service.";
                return RedirectToAction(nameof(Create), new { patientId = SelectedPatientId.Value });
            }

            // Services only registers the patient's chosen service. The actual
            // clinical form is completed later from Consultation.
            if (SelectedServices == "Prenatal")
            {
                _context.PrenatalRecords.Add(new PrenatalRecord
                {
                    PatientId = SelectedPatientId.Value,
                    RecordDate = DateTime.Today,
                    SelectedServices = SelectedServices
                });
            }
            else if (SelectedServices == "Normal Delivery Fee & Newborn Care Package")
            {
                _context.NewbornRecords.Add(new NewbornRecord
                {
                    PatientId = SelectedPatientId.Value
                });
            }
            else
            {
                _context.FamilyPlanningRecords.Add(new FamilyPlanningRecord
                {
                    PatientId = SelectedPatientId.Value,
                    RecordDate = DateTime.Today,
                    SelectedService = SelectedServices
                });
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "The patient's selected service has been registered.";
            return RedirectToAction(nameof(Index));
        }

        // Changes only the service registration. Clinical/interview fields are
        // intentionally edited from Consultation, not from the Services list.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditRegistration(int id, string type, string? selectedService)
        {
            selectedService = selectedService?.Trim();
            if (string.IsNullOrWhiteSpace(selectedService)
                || !AvailableServices.Contains(selectedService))
            {
                TempData["ErrorMessage"] = "Please select a valid service.";
                return RedirectToAction(nameof(Index));
            }

            var linkedConsultations = _context.Consultations
                .Include(c => c.Appointment)
                .Where(c => c.RecordId == id && c.RecordType == type)
                .ToList();

            if (linkedConsultations.Any(c => c.Status == "Completed"))
            {
                TempData["ErrorMessage"] = "This service can no longer be changed because its consultation is already completed.";
                return RedirectToAction(nameof(Index));
            }

            PrenatalRecord? prenatalRecord = null;
            NewbornRecord? newbornRecord = null;
            FamilyPlanningRecord? familyPlanningRecord = null;
            int? patientId = null;
            DateTime registrationDate;

            if (type == "Prenatal")
            {
                prenatalRecord = _context.PrenatalRecords.FirstOrDefault(r => r.Id == id);
                if (prenatalRecord == null) return NotFound();
                patientId = prenatalRecord.PatientId;
                registrationDate = prenatalRecord.RecordDate ?? DateTime.Today;
            }
            else if (type == "Newborn")
            {
                newbornRecord = _context.NewbornRecords.FirstOrDefault(r => r.Id == id);
                if (newbornRecord == null) return NotFound();
                patientId = newbornRecord.PatientId;
                registrationDate = newbornRecord.DateTimeOfAdmission ?? DateTime.Now;
            }
            else if (type == "Family Planning")
            {
                familyPlanningRecord = _context.FamilyPlanningRecords.FirstOrDefault(r => r.Id == id);
                if (familyPlanningRecord == null) return NotFound();
                patientId = familyPlanningRecord.PatientId;
                registrationDate = familyPlanningRecord.RecordDate ?? DateTime.Today;
            }
            else
            {
                return NotFound();
            }

            if (!patientId.HasValue || !_context.Patients.Any(p => p.Id == patientId.Value))
            {
                TempData["ErrorMessage"] = "The registered patient for this service could not be found.";
                return RedirectToAction(nameof(Index));
            }

            var targetType = GetRecordType(selectedService);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var resultingRecordId = id;

                if (targetType == type)
                {
                    if (prenatalRecord != null)
                        prenatalRecord.SelectedServices = "Prenatal";
                    else if (familyPlanningRecord != null)
                        familyPlanningRecord.SelectedService = selectedService;
                    // A NewbornRecord always represents the single newborn package.
                }
                else
                {
                    if (targetType == "Prenatal")
                    {
                        var replacement = new PrenatalRecord
                        {
                            PatientId = patientId.Value,
                            RecordDate = registrationDate.Date,
                            SelectedServices = "Prenatal"
                        };
                        _context.PrenatalRecords.Add(replacement);
                        _context.SaveChanges();
                        resultingRecordId = replacement.Id;
                    }
                    else if (targetType == "Newborn")
                    {
                        var replacement = new NewbornRecord
                        {
                            PatientId = patientId.Value,
                            DateTimeOfAdmission = registrationDate
                        };
                        _context.NewbornRecords.Add(replacement);
                        _context.SaveChanges();
                        resultingRecordId = replacement.Id;
                    }
                    else
                    {
                        var replacement = new FamilyPlanningRecord
                        {
                            PatientId = patientId.Value,
                            RecordDate = registrationDate.Date,
                            SelectedService = selectedService
                        };
                        _context.FamilyPlanningRecords.Add(replacement);
                        _context.SaveChanges();
                        resultingRecordId = replacement.Id;
                    }

                    if (prenatalRecord != null)
                        _context.PrenatalRecords.Remove(prenatalRecord);
                    else if (newbornRecord != null)
                        _context.NewbornRecords.Remove(newbornRecord);
                    else if (familyPlanningRecord != null)
                        _context.FamilyPlanningRecords.Remove(familyPlanningRecord);
                }

                // An in-progress consultation follows the newly selected service.
                // Continue will therefore open the matching clinical form.
                foreach (var consultation in linkedConsultations)
                {
                    consultation.ServiceType = selectedService;
                    consultation.RecordType = targetType;
                    consultation.RecordId = resultingRecordId;
                    if (consultation.Appointment != null)
                        consultation.Appointment.ServiceType = selectedService;
                }

                _context.SaveChanges();
                transaction.Commit();
                TempData["SuccessMessage"] = "Service registration updated successfully.";
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The service registration could not be updated. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static string GetRecordType(string serviceName)
        {
            if (serviceName == "Prenatal") return "Prenatal";
            if (serviceName == "Normal Delivery Fee & Newborn Care Package") return "Newborn";
            return "Family Planning";
        }

        // --- DETAILS ---
        [HttpGet]
        public IActionResult Details(int id, string type)
        {
            var model = new ServiceFormViewModel { RecordType = type, IsViewOnly = true };
            if (type == "Prenatal")
            {
                model.PrenatalRecord = _context.PrenatalRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.PrenatalRecord == null) return NotFound();
                ViewBag.Patient = model.PrenatalRecord.Patient;

                if (!string.IsNullOrEmpty(model.PrenatalRecord.PrenatalVisitsJson))
                    model.PrenatalRecord.PrenatalVisits = System.Text.Json.JsonSerializer.Deserialize<List<PrenatalVisit>>(model.PrenatalRecord.PrenatalVisitsJson) ?? new List<PrenatalVisit>();
            }
            else if (type == "Newborn")
            {
                model.NewbornRecord = _context.NewbornRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.NewbornRecord == null) return NotFound();
                ViewBag.Patient = model.NewbornRecord.Patient;

                // Deserialize JSON columns back to lists for display
                if (!string.IsNullOrEmpty(model.NewbornRecord.VitalsJson))
                    model.NewbornRecord.Vitals = System.Text.Json.JsonSerializer.Deserialize<List<NewbornVital>>(model.NewbornRecord.VitalsJson) ?? new List<NewbornVital>();
                if (!string.IsNullOrEmpty(model.NewbornRecord.MedicationsJson))
                    model.NewbornRecord.Medications = System.Text.Json.JsonSerializer.Deserialize<List<NewbornMedication>>(model.NewbornRecord.MedicationsJson) ?? new List<NewbornMedication>();
            }
            else if (type == "Family Planning")
            {
                model.FamilyPlanningRecord = _context.FamilyPlanningRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.FamilyPlanningRecord == null) return NotFound();
                ViewBag.Patient = model.FamilyPlanningRecord.Patient;
            }
            else return NotFound();

            return View(model);
        }

        // --- EDIT (GET) ---
        [HttpGet]
        public IActionResult Edit(int id, string type)
        {
            var model = new ServiceFormViewModel { RecordType = type, IsViewOnly = false };
            ViewBag.Patients = _context.Patients.OrderBy(p => p.FullName).ToList();
            if (type == "Prenatal")
            {
                model.PrenatalRecord = _context.PrenatalRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.PrenatalRecord == null) return NotFound();
                ViewBag.Patient = model.PrenatalRecord.Patient;
                
                if (!string.IsNullOrEmpty(model.PrenatalRecord.PrenatalVisitsJson))
                    model.PrenatalRecord.PrenatalVisits = System.Text.Json.JsonSerializer.Deserialize<List<PrenatalVisit>>(model.PrenatalRecord.PrenatalVisitsJson) ?? new List<PrenatalVisit>();
            }
            else if (type == "Newborn")
            {
                model.NewbornRecord = _context.NewbornRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.NewbornRecord == null) return NotFound();
                ViewBag.Patient = model.NewbornRecord.Patient;

                // Deserialize JSON columns back to lists for editing
                if (!string.IsNullOrEmpty(model.NewbornRecord.VitalsJson))
                    model.NewbornRecord.Vitals = System.Text.Json.JsonSerializer.Deserialize<List<NewbornVital>>(model.NewbornRecord.VitalsJson) ?? new List<NewbornVital>();
                if (!string.IsNullOrEmpty(model.NewbornRecord.MedicationsJson))
                    model.NewbornRecord.Medications = System.Text.Json.JsonSerializer.Deserialize<List<NewbornMedication>>(model.NewbornRecord.MedicationsJson) ?? new List<NewbornMedication>();
            }
            else if (type == "Family Planning")
            {
                model.FamilyPlanningRecord = _context.FamilyPlanningRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.FamilyPlanningRecord == null) return NotFound();
                ViewBag.Patient = model.FamilyPlanningRecord.Patient;
            }
            else return NotFound();

            return View(model);
        }

        // --- EDIT (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ServiceFormViewModel model)
        {
            ModelState.Clear();
            string type = model.RecordType;

            if (type == "Prenatal")
            {
                var existing = _context.PrenatalRecords.FirstOrDefault(r => r.Id == id);
                if (existing == null) return NotFound();

                if (model.PrenatalRecord?.PrenatalVisits != null && model.PrenatalRecord.PrenatalVisits.Count > 0)
                    model.PrenatalRecord.PrenatalVisitsJson = System.Text.Json.JsonSerializer.Serialize(model.PrenatalRecord.PrenatalVisits);

                _context.Entry(existing).CurrentValues.SetValues(model.PrenatalRecord!);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Prenatal record updated successfully.";
            }
            else if (type == "Newborn")
            {
                var existing = _context.NewbornRecords.FirstOrDefault(r => r.Id == id);
                if (existing == null) return NotFound();

                if (model.NewbornRecord?.Vitals != null)
                    model.NewbornRecord.VitalsJson = model.NewbornRecord.Vitals.Count > 0
                        ? System.Text.Json.JsonSerializer.Serialize(model.NewbornRecord.Vitals)
                        : null;
                if (model.NewbornRecord?.Medications != null)
                    model.NewbornRecord.MedicationsJson = model.NewbornRecord.Medications.Count > 0
                        ? System.Text.Json.JsonSerializer.Serialize(model.NewbornRecord.Medications)
                        : null;

                _context.Entry(existing).CurrentValues.SetValues(model.NewbornRecord!);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Newborn record updated successfully.";
            }
            else if (type == "Family Planning")
            {
                var existing = _context.FamilyPlanningRecords.FirstOrDefault(r => r.Id == id);
                if (existing == null) return NotFound();

                _context.Entry(existing).CurrentValues.SetValues(model.FamilyPlanningRecord!);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Family Planning record updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // --- DELETE (GET) ---
        [HttpGet]
        public IActionResult Delete(int id, string type)
        {
            var model = new ServiceFormViewModel { RecordType = type };
            if (type == "Prenatal")
            {
                model.PrenatalRecord = _context.PrenatalRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.PrenatalRecord == null) return NotFound();
                ViewBag.Patient = model.PrenatalRecord.Patient;

                if (!string.IsNullOrEmpty(model.PrenatalRecord.PrenatalVisitsJson))
                    model.PrenatalRecord.PrenatalVisits = System.Text.Json.JsonSerializer.Deserialize<List<PrenatalVisit>>(model.PrenatalRecord.PrenatalVisitsJson) ?? new List<PrenatalVisit>();
            }
            else if (type == "Newborn")
            {
                model.NewbornRecord = _context.NewbornRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.NewbornRecord == null) return NotFound();
                ViewBag.Patient = model.NewbornRecord.Patient;
            }
            else if (type == "Family Planning")
            {
                model.FamilyPlanningRecord = _context.FamilyPlanningRecords.Include(r => r.Patient).FirstOrDefault(r => r.Id == id);
                if (model.FamilyPlanningRecord == null) return NotFound();
                ViewBag.Patient = model.FamilyPlanningRecord.Patient;
            }
            else return NotFound();

            return View(model);
        }

        // --- DELETE (POST) ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, string type)
        {
            if (type == "Prenatal")
            {
                var record = _context.PrenatalRecords.Find(id);
                if (record != null) { _context.PrenatalRecords.Remove(record); _context.SaveChanges(); }
            }
            else if (type == "Newborn")
            {
                var record = _context.NewbornRecords.Find(id);
                if (record != null) { _context.NewbornRecords.Remove(record); _context.SaveChanges(); }
            }
            else if (type == "Family Planning")
            {
                var record = _context.FamilyPlanningRecords.Find(id);
                if (record != null) { _context.FamilyPlanningRecords.Remove(record); _context.SaveChanges(); }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
