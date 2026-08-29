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

            return View(patient);
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
        /// POST — I-delete ang pasyente (ug ang iyang mga prenatal records) gikan sa database.
        /// Ang cascade delete sa DbContext mag-atiman sa related PrenatalRecords.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // 1. Get all patients except the one to be deleted, ordered by current Id
            var remainingPatients = _context.Patients
                .Where(p => p.Id != id)
                .OrderBy(p => p.Id)
                .ToList();

            // 2. Get all prenatal records except those belonging to the deleted patient
            var remainingRecords = _context.PrenatalRecords
                .Where(r => r.PatientId != id)
                .OrderBy(r => r.Id)
                .ToList();

            // 3. Clear both tables completely
            _context.PrenatalRecords.RemoveRange(_context.PrenatalRecords);
            _context.Patients.RemoveRange(_context.Patients);
            _context.SaveChanges();

            // 4. Reseed identity to 0 so the next insert starts at 1
            _context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Patients', RESEED, 0);");
            _context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PrenatalRecords', RESEED, 0);");

            // 5. Re-insert patients and map old ID to new ID
            var idMap = new Dictionary<int, int>();
            foreach (var patient in remainingPatients)
            {
                int oldId = patient.Id;
                var newPatient = new Patient
                {
                    FullName = patient.FullName,
                    Address = patient.Address,
                    Age = patient.Age,
                    MaritalStatus = patient.MaritalStatus,
                    DateOfBirth = patient.DateOfBirth,
                    Religion = patient.Religion,
                    LMP = patient.LMP,
                    AOG = patient.AOG,
                    EDC = patient.EDC,
                    Menarche = patient.Menarche,
                    ContactNo = patient.ContactNo,
                    Gravida = patient.Gravida,
                    TFAL = patient.TFAL,
                    Occupation = patient.Occupation
                };
                _context.Patients.Add(newPatient);
                _context.SaveChanges(); // Save changes immediately to get the new sequential Id populated

                idMap[oldId] = newPatient.Id;
            }

            // 6. Re-insert prenatal records with mapped PatientId
            foreach (var record in remainingRecords)
            {
                int? newPatientId = null;
                if (record.PatientId.HasValue && idMap.ContainsKey(record.PatientId.Value))
                {
                    newPatientId = idMap[record.PatientId.Value];
                }

                var newRecord = new PrenatalRecord
                {
                    PatientId = newPatientId,
                    RecordDate = record.RecordDate,
                    AOG = record.AOG,
                    Weight = record.Weight,
                    BloodPressure = record.BloodPressure,
                    Temperature = record.Temperature,
                    FundalHeight = record.FundalHeight,
                    FetalHeartTone = record.FetalHeartTone,
                    Remarks = record.Remarks,
                    SelectedServices = record.SelectedServices
                };
                _context.PrenatalRecords.Add(newRecord);
            }
            _context.SaveChanges();

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
