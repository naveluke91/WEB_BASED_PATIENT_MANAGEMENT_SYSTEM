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

            // ==== [ST-PATIENT-16] START — Archive: itago ang archived (o ipakita ra sila kung ?showArchived=true) ====
            /*
            bool showArchived = Request.Query["showArchived"] == "true";
            patients = patients.Where(p => p.IsArchived == showArchived).ToList();
            ViewBag.StShowArchived = showArchived;
            */
            // ==== [ST-PATIENT-16] END ====

            // ==== [ST-PATIENT-10] START — Age ug AOG nga mo-update (i-compute gikan sa DOB ug LMP karon) ====
            // Dili ni i-save sa database; para sa display ra (walay SaveChanges dinhi).
            /*
            foreach (var p in patients)
            {
                if (p.DateOfBirth.HasValue) p.Age = Patient.CalculateAge(p.DateOfBirth.Value);
                p.AOG = Patient.StAogFromLmp(p.LMP) ?? p.AOG;
            }
            */
            // ==== [ST-PATIENT-10] END ====

            // ==== [ST-PATIENT-2] START — Sort (Ascending / Descending) gikan sa ?sortOrder= ====
            // OrderBy = ascending (A–Z, 1–9, karaan una); OrderByDescending = descending.
            /*
            string sortOrder = Request.Query["sortOrder"].ToString();
            patients = sortOrder switch
            {
                "name_desc"       => patients.OrderByDescending(p => p.FullName).ToList(),
                "age_asc"         => patients.OrderBy(p => p.Age).ToList(),
                "age_desc"        => patients.OrderByDescending(p => p.Age).ToList(),
                "edc_asc"         => patients.OrderBy(p => p.EDC ?? DateTime.MaxValue).ToList(),
                "edc_desc"        => patients.OrderByDescending(p => p.EDC ?? DateTime.MinValue).ToList(),
                "registered_desc" => patients.OrderByDescending(p => p.DateRegistered ?? DateTime.MinValue).ToList(),
                "registered_asc"  => patients.OrderBy(p => p.DateRegistered ?? DateTime.MaxValue).ToList(),
                _                 => patients // default: Name A–Z (gikan sa query sa taas)
            };
            ViewBag.StSortOrder = sortOrder;
            */
            // ==== [ST-PATIENT-2] END ====

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
        public IActionResult Create([Bind(Patient.FormFields)] Patient patient)
        {
            // I-trim ug i-validate pag-usab sa server; ang edad gikan sa DOB.
            patient.TrimTextFields();
            patient.Age = patient.DateOfBirth.HasValue ? Patient.CalculateAge(patient.DateOfBirth.Value) : 0;
            ModelState.Clear();
            var errors = Patient.ValidateInput(patient);

            // ==== [ST-PATIENT-11] START — Duplicate check: parehas nga Full Name + Contact No. ====
            /*
            if (StIsDuplicatePatient(patient))
                errors.TryAdd(nameof(Patient.FullName), "A patient with this name and contact number is already registered.");
            */
            // ==== [ST-PATIENT-11] END ====

            // ==== [ST-PATIENT-12] START — Bag-ong field: Blood Type (dropdown, optional) ====
            // Ang [Bind(Patient.FormFields)] dili mo-apil sa BloodType, so basahon gikan sa Request.Form.
            /*
            patient.BloodType = string.IsNullOrWhiteSpace(Request.Form["BloodType"]) ? null : Request.Form["BloodType"].ToString().Trim();
            if (patient.BloodType != null && !Patient.BloodTypes.Contains(patient.BloodType))
                errors.TryAdd("BloodType", "Please select a valid blood type.");
            */
            // ==== [ST-PATIENT-12] END ====

            // ==== [ST-PATIENT-13] START — Bag-ong field: Email Address (optional) ====
            /*
            patient.Email = string.IsNullOrWhiteSpace(Request.Form["Email"]) ? null : Request.Form["Email"].ToString().Trim().ToLowerInvariant();
            if (patient.Email != null && (patient.Email.Length > 256 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(patient.Email)))
                errors.TryAdd("Email", "Enter a valid email address.");
            */
            // ==== [ST-PATIENT-13] END ====

            if (errors.Count == 0)
            {
                // I-add ang bag-ong pasyente sa database ug i-save
                _context.Patients.Add(patient);
                _context.SaveChanges();

                // ==== [ST-USER-8] START — Audit trail: Add Patient ====
                /*
                StAudit.Log(_context, User.Identity?.Name, "Add Patient", $"Added patient \"{patient.FullName}\" (ID {patient.Id}).");
                */
                // ==== [ST-USER-8] END ====

                TempData["SuccessMessage"] = $"Patient \"{patient.FullName}\" was added.";
                return RedirectToAction(nameof(Index));
            }

            // Ablihi pag-usab ang modal uban ang sayop matag field.
            TempData["OpenAddModal"] = true;
            TempData["PatientForm"] = PatientFormState(patient, errors);
            return RedirectToAction(nameof(Index));
        }

        // Datos para ma-fill balik ang modal ug ma-marka ang sayop nga field.
        private static string PatientFormState(Patient patient, Dictionary<string, string> errors) =>
            System.Text.Json.JsonSerializer.Serialize(new
            {
                values = new Dictionary<string, string?>
                {
                    [nameof(Patient.FullName)] = patient.FullName,
                    [nameof(Patient.Address)] = patient.Address,
                    [nameof(Patient.DateOfBirth)] = patient.DateOfBirth?.ToString("yyyy-MM-dd"),
                    [nameof(Patient.MaritalStatus)] = patient.MaritalStatus,
                    [nameof(Patient.Religion)] = patient.Religion,
                    [nameof(Patient.Occupation)] = patient.Occupation,
                    [nameof(Patient.ContactNo)] = patient.ContactNo,
                    [nameof(Patient.LMP)] = patient.LMP?.ToString("yyyy-MM-dd"),
                    [nameof(Patient.AOG)] = patient.AOG,
                    [nameof(Patient.EDC)] = patient.EDC?.ToString("yyyy-MM-dd"),
                    [nameof(Patient.Menarche)] = patient.Menarche,
                    [nameof(Patient.Gravida)] = patient.Gravida,
                    [nameof(Patient.TFAL)] = patient.TFAL
                },
                errors
            });

        /// <summary>
        /// GET — Ibalik ang detalye sa usa ka pasyente sa JSON format para sa modal.
        /// </summary>
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            // ==== [ST-PATIENT-10] START — Age ug AOG nga updated sa View/Edit modal ====
            /*
            if (patient.DateOfBirth.HasValue) patient.Age = Patient.CalculateAge(patient.DateOfBirth.Value);
            patient.AOG = Patient.StAogFromLmp(patient.LMP) ?? patient.AOG;
            */
            // ==== [ST-PATIENT-10] END ====

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

        // ==== [ST-PATIENT-12] START — GET /Patients/StBloodType/5 (para sa Edit ug View modal) ====
        /*
        [HttpGet]
        public IActionResult StBloodType(int id)
        {
            var patient = _context.Patients.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();
            return Json(new { bloodType = patient.BloodType ?? "" });
        }
        */
        // ==== [ST-PATIENT-12] END ====

        // ==== [ST-PATIENT-13] START — GET /Patients/StEmail/5 (para sa Edit ug View modal) ====
        /*
        [HttpGet]
        public IActionResult StEmail(int id)
        {
            var patient = _context.Patients.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();
            return Json(new { email = patient.Email ?? "" });
        }
        */
        // ==== [ST-PATIENT-13] END ====

        // ==== [ST-PATIENT-14] START — GET /Patients/History/5: tanang record sa usa ka pasyente ====
        // Appointments, Services, Consultations ug Payments — para sa View Details modal.
        /*
        [HttpGet]
        public IActionResult History(int id)
        {
            if (!_context.Patients.Any(p => p.Id == id)) return NotFound();

            var appointments = _context.Appointments.AsNoTracking()
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime)
                .ToList()
                .Select(a => new { date = a.AppointmentDate.ToString("MMM dd, yyyy"), time = a.FormattedTime, status = a.Status, service = a.ServiceType ?? "—" });

            var services = _context.Services.AsNoTracking()
                .Where(s => s.PatientId == id)
                .OrderByDescending(s => s.Id)
                .Select(s => new { service = s.ServiceName, price = s.Price, type = s.AppointmentId == null ? "Walk-In" : "Appointment" })
                .ToList();

            var consultations = _context.Consultations.AsNoTracking()
                .Where(c => c.PatientId == id)
                .OrderByDescending(c => c.StartedAt)
                .ToList()
                .Select(c => new { date = (c.CompletedAt ?? c.StartedAt).ToString("MMM dd, yyyy"), service = c.ServiceType, visitType = c.VisitType, status = c.Status });

            var payments = _context.Payments.AsNoTracking()
                .Where(p => p.Consultation != null && p.Consultation.PatientId == id)
                .Include(p => p.Consultation)
                .OrderByDescending(p => p.PaymentDate)
                .ToList()
                .Select(p => new { date = p.PaymentDate.ToString("MMM dd, yyyy"), service = p.Consultation?.ServiceType ?? "—", amount = p.Amount ?? 0, method = p.PaymentMethod });

            return Json(new { appointments, services, consultations, payments });
        }
        */
        // ==== [ST-PATIENT-14] END ====

        // ==== [ST-PATIENT-16] START — POST /Patients/Archive/5 ug /Patients/Restore/5 ====
        // Archive = itago sa listahan pero dili i-delete (ma-restore pa).
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Archive(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            patient.IsArchived = true;
            patient.ArchivedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Patient \"{patient.FullName}\" was archived.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null) return NotFound();

            patient.IsArchived = false;
            patient.ArchivedAt = null;
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Patient \"{patient.FullName}\" was restored.";
            return RedirectToAction(nameof(Index), new { showArchived = "true" });
        }
        */
        // ==== [ST-PATIENT-16] END ====

        // ==== [ST-PATIENT-11] START — Helper: naa na bay pasyente nga parehas og ngalan ug contact? ====
        /*
        private bool StIsDuplicatePatient(Patient patient)
        {
            static string Normalize(string? value) =>
                string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

            var name = Normalize(patient.FullName);
            var contact = (patient.ContactNo ?? string.Empty).Trim();

            return _context.Patients.AsNoTracking()
                .Where(p => p.Id != patient.Id && p.ContactNo == contact)
                .AsEnumerable()
                .Any(p => Normalize(p.FullName) == name);
        }
        */
        // ==== [ST-PATIENT-11] END ====

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

            // I-trim ug i-validate pag-usab sa server; ang edad gikan sa DOB.
            patient.TrimTextFields();
            patient.Age = patient.DateOfBirth.HasValue ? Patient.CalculateAge(patient.DateOfBirth.Value) : 0;
            ModelState.Clear();
            var errors = Patient.ValidateInput(patient);

            // ==== [ST-PATIENT-11] START — Duplicate check (dili apil ang kaugalingon nga record) ====
            /*
            if (StIsDuplicatePatient(patient))
                errors.TryAdd(nameof(Patient.FullName), "A patient with this name and contact number is already registered.");
            */
            // ==== [ST-PATIENT-11] END ====

            // ==== [ST-PATIENT-12] START — Blood Type validation ====
            /*
            patient.BloodType = string.IsNullOrWhiteSpace(patient.BloodType) ? null : patient.BloodType.Trim();
            if (patient.BloodType != null && !Patient.BloodTypes.Contains(patient.BloodType))
                errors.TryAdd("BloodType", "Please select a valid blood type.");
            */
            // ==== [ST-PATIENT-12] END ====

            // ==== [ST-PATIENT-13] START — Email validation ====
            /*
            patient.Email = string.IsNullOrWhiteSpace(patient.Email) ? null : patient.Email.Trim().ToLowerInvariant();
            if (patient.Email != null && (patient.Email.Length > 256 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(patient.Email)))
                errors.TryAdd("Email", "Enter a valid email address.");
            */
            // ==== [ST-PATIENT-13] END ====

            if (errors.Count == 0)
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

                // ==== [ST-PATIENT-12] START — I-save ang Blood Type ====
                /*
                existing.BloodType = patient.BloodType;
                */
                // ==== [ST-PATIENT-12] END ====

                // ==== [ST-PATIENT-13] START — I-save ang Email ====
                /*
                existing.Email = patient.Email;
                */
                // ==== [ST-PATIENT-13] END ====

                // I-save ang mga pagbag-o sa database
                _context.SaveChanges();

                // ==== [ST-USER-8] START — Audit trail: Edit Patient ====
                /*
                StAudit.Log(_context, User.Identity?.Name, "Edit Patient", $"Updated patient \"{existing.FullName}\" (ID {existing.Id}).");
                */
                // ==== [ST-USER-8] END ====

                TempData["SuccessMessage"] = $"Patient \"{patient.FullName}\" was updated.";
                return RedirectToAction(nameof(Index));
            }

            // The edit form is displayed in the Index modal, so reopen it after validation fails.
            TempData["OpenEditModalId"] = id;
            TempData["PatientForm"] = PatientFormState(patient, errors);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET — Ipakita ang confirmation page para sa pag-delete sa pasyente.
        /// </summary>
        // ==== [ST-PATIENT-17] START — Admin ra ang maka-delete og pasyente ====
        /*
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = UserRoles.Admin)]
        */
        // ==== [ST-PATIENT-17] END ====
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
        // ==== [ST-PATIENT-17] START — Admin ra ang maka-delete og pasyente ====
        /*
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = UserRoles.Admin)]
        */
        // ==== [ST-PATIENT-17] END ====
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

                // ==== [ST-USER-8] START — Audit trail: Delete Patient ====
                /*
                StAudit.Log(_context, User.Identity?.Name, "Delete Patient", $"Deleted patient \"{patient.FullName}\" (ID {id}) and related records.");
                */
                // ==== [ST-USER-8] END ====
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
        [Microsoft.AspNetCore.Authorization.AllowAnonymous] // Makita bisan wala naka-login.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
