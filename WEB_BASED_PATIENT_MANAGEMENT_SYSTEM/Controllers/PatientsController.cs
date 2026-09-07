using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// PatientsController — mag-handle sa tanan nga CRUD operations para sa mga pasyente.
    /// Konektado na sa SQL Server database pinaagi sa ApplicationDbContext.
    /// </summary>
    public class PatientsController : Controller
    {
        // Ang _context mao ang koneksyon sa database
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor — ang DbContext gi-inject diri pinaagi sa Dependency Injection.
        /// I-register kini sa Program.cs gamit ang builder.Services.AddDbContext.
        /// </summary>
        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ibalik ang lista sa tanan nga pasyente gikan sa database.
        /// Gi-order pinaagi sa pangalan (alphabetical order).
        /// </summary>
        public IActionResult Index()
        {
            // Kuha-on ang tanan nga pasyente gikan sa database, i-order sa pangalan
            var patients = _context.Patients
                .OrderBy(p => p.FullName)
                .ToList();

            return View(patients);
        }

        /// <summary>
        /// GET — Ipakita ang form para mag-register ug bag-ong pasyente.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST — I-save ang bag-ong pasyente sa database.
        /// I-calculate una ang edad base sa petsa sa pagkatawo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Patient patient)
        {
            // Ang age awtomatikong kalkulado gikan sa gipili nga date of birth.
            ModelState.Remove(nameof(Patient.Age));
            if (patient.DateOfBirth.HasValue)
            {
                patient.Age = Patient.CalculateAge(patient.DateOfBirth.Value);

                if (patient.DateOfBirth.Value.Date > DateTime.Today)
                {
                    ModelState.AddModelError(nameof(Patient.DateOfBirth), "Date of birth cannot be in the future.");
                }
                else if (patient.Age > 130)
                {
                    ModelState.AddModelError(nameof(Patient.DateOfBirth), "Age must be between 0 and 130.");
                }
            }

            if (ModelState.IsValid)
            {
                // I-add ang bag-ong pasyente sa database ug i-save
                _context.Patients.Add(patient);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Ang pasyente nga \"{patient.FullName}\" nalista na sa sistema.";
                return RedirectToAction(nameof(Index));
            }

            // ModelState invalid — redirect back to Index and re-open the modal
            TempData["OpenAddModal"] = true;
            TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET — Ibalik ang detalye sa usa ka pasyente sa JSON format para sa modal.
        /// </summary>
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            return Json(new
            {
                id = patient.Id,
                fullName = patient.FullName,
                address = patient.Address,
                dateOfBirth = patient.DateOfBirth?.ToString("MMMM dd, yyyy") ?? "—",
                dateOfBirthRaw = patient.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                age = patient.Age,
                maritalStatus = string.IsNullOrEmpty(patient.MaritalStatus) ? "—" : patient.MaritalStatus,
                religion = string.IsNullOrEmpty(patient.Religion) ? "—" : patient.Religion,
                lmp = patient.LMP?.ToString("MMMM dd, yyyy") ?? "—",
                lmpRaw = patient.LMP?.ToString("yyyy-MM-dd") ?? "",
                aog = string.IsNullOrEmpty(patient.AOG) ? "—" : patient.AOG,
                aogRaw = string.IsNullOrEmpty(patient.AOG) ? "" : patient.AOG,
                edc = patient.EDC?.ToString("MMMM dd, yyyy") ?? "—",
                edcRaw = patient.EDC?.ToString("yyyy-MM-dd") ?? "",
                menarche = string.IsNullOrEmpty(patient.Menarche) ? "—" : patient.Menarche,
                menarcheRaw = string.IsNullOrEmpty(patient.Menarche) ? "" : patient.Menarche,
                contactNo = string.IsNullOrEmpty(patient.ContactNo) ? "—" : patient.ContactNo,
                contactNoRaw = string.IsNullOrEmpty(patient.ContactNo) ? "" : patient.ContactNo,
                gravida = string.IsNullOrEmpty(patient.Gravida) ? "—" : patient.Gravida,
                gravidaRaw = string.IsNullOrEmpty(patient.Gravida) ? "" : patient.Gravida,
                tfal = string.IsNullOrEmpty(patient.TFAL) ? "—" : patient.TFAL,
                tfalRaw = string.IsNullOrEmpty(patient.TFAL) ? "" : patient.TFAL,
                occupation = string.IsNullOrEmpty(patient.Occupation) ? "—" : patient.Occupation,
                occupationRaw = string.IsNullOrEmpty(patient.Occupation) ? "" : patient.Occupation
            });
        }

        /// <summary>
        /// GET — Ipakita ang detalye sa usa ka pasyente base sa Id.
        /// </summary>
        [HttpGet]
        public IActionResult Details(int id)
        {
            // Pangitaon ang pasyente sa database base sa Id
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        /// <summary>
        /// GET — Ipakita ang edit form para sa usa ka pasyente base sa Id.
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Pangitaon ang pasyente sa database
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        /// <summary>
        /// POST — I-update ang datos sa pasyente sa database.
        /// I-calculate pag-usab ang edad base sa bag-ong petsa sa pagkatawo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Patient patient)
        {
            // Sigurohon nga ang Id sa URL ug sa form mao ra ang usa
            if (id != patient.Id) return NotFound();

            // Ang age awtomatikong kalkulado gikan sa gipili nga date of birth.
            ModelState.Remove(nameof(Patient.Age));
            if (patient.DateOfBirth.HasValue)
            {
                patient.Age = Patient.CalculateAge(patient.DateOfBirth.Value);

                if (patient.DateOfBirth.Value.Date > DateTime.Today)
                {
                    ModelState.AddModelError(nameof(Patient.DateOfBirth), "Date of birth cannot be in the future.");
                }
                else if (patient.Age > 130)
                {
                    ModelState.AddModelError(nameof(Patient.DateOfBirth), "Age must be between 0 and 130.");
                }
            }

            if (ModelState.IsValid)
            {
                // Pangitaon ang existing nga record sa database
                var existing = _context.Patients.FirstOrDefault(p => p.Id == id);
                if (existing == null) return NotFound();

                // I-update ang matag field
                existing.FullName = patient.FullName;
                existing.Address = patient.Address;
                existing.DateOfBirth = patient.DateOfBirth;
                existing.Age = patient.Age;
                existing.MaritalStatus = patient.MaritalStatus;
                existing.Religion = patient.Religion;
                existing.LMP = patient.LMP;
                existing.AOG = patient.AOG;
                existing.EDC = patient.EDC;
                existing.Menarche = patient.Menarche;
                existing.ContactNo = patient.ContactNo;
                existing.Gravida = patient.Gravida;
                existing.TFAL = patient.TFAL;
                existing.Occupation = patient.Occupation;

                // I-save ang mga pagbag-o sa database
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Ang rekord ni \"{patient.FullName}\" na-update na.";
                return RedirectToAction(nameof(Index));
            }

            // The edit form is displayed in the Index modal, so reopen it after validation fails.
            TempData["OpenEditModalId"] = id;
            TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET — Ipakita ang confirmation page para sa pag-delete sa pasyente.
        /// </summary>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Pangitaon ang pasyente sa database
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        /// <summary>
        /// POST — I-delete ang pasyente ug tanan niyang related records gikan sa database.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Remove every dependent record first. The database uses restrictive
                // foreign keys for these tables, so the patient is removed last.
                _context.Consultations.RemoveRange(_context.Consultations
                    .Where(c => c.PatientId == id)
                    .ToList());
                _context.PrenatalRecords.RemoveRange(_context.PrenatalRecords
                    .Where(r => r.PatientId == id)
                    .ToList());
                _context.NewbornRecords.RemoveRange(_context.NewbornRecords
                    .Where(r => r.PatientId == id)
                    .ToList());
                _context.FamilyPlanningRecords.RemoveRange(_context.FamilyPlanningRecords
                    .Where(r => r.PatientId == id)
                    .ToList());
                _context.Services.RemoveRange(_context.Services
                    .Where(s => s.PatientId == id)
                    .ToList());
                _context.Appointments.RemoveRange(_context.Appointments
                    .Where(a => a.PatientId == id)
                    .ToList());
                _context.SaveChanges();

                _context.Patients.Remove(patient);
                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The patient and related records could not be deleted. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Ipakita ang error page kapag may nangyari nga exception sa app.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
