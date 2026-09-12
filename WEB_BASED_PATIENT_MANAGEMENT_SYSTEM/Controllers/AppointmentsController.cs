using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// AppointmentsController — handles all CRUD for appointments.
    /// </summary>
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------------
        // GET /Appointments
        // -----------------------------------------------------------------------
        public IActionResult Index()
        {
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/SearchPatients?q=Juan
        // Searches registered Patients by name for the Confirm modal.
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult SearchPatients(string? q = null)
        {
            var lower = (q ?? "").ToLower().Trim();
            var patients = _context.Patients
                .AsEnumerable()
                .Where(p => string.IsNullOrEmpty(lower) || p.FullName.ToLower().Contains(lower))
                .OrderBy(p => p.FullName)
                .Select(p => new { p.Id, p.FullName, p.ContactNo })
                .Take(10)
                .ToList();
            return Json(patients);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/GetPatients — JSON list for patient dropdown
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult GetPatients()
        {
            var patients = _context.Patients
                .OrderBy(p => p.FullName)
                .Select(p => new { p.Id, p.FullName, p.ContactNo })
                .ToList();
            return Json(patients);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/SearchAppointments?q=Juan
        // "Existing Appointment" = create a new appointment for an ALREADY
        // REGISTERED patient, so the search source of truth is the Patients
        // table — NOT previous Appointments. A patient does not need an old
        // appointment to appear here.
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult SearchAppointments(string? q = null)
        {
            var lower = (q ?? "").ToLower().Trim();

            var results = _context.Patients
                .AsEnumerable()
                .Where(p => string.IsNullOrEmpty(lower) || p.FullName.ToLower().Contains(lower))
                .OrderBy(p => p.FullName)
                .Select(p => new
                {
                    patientId   = p.Id,
                    patientName = p.FullName,
                    contactNo   = p.ContactNo
                })
                .Take(10)
                .ToList();

            return Json(results);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/GetBookedTimes?date=2026-08-06[&excludeId=3]
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult GetBookedTimes(string date, int? excludeId = null)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date.");

            var targetDate = parsedDate.Date;

            var booked = _context.Appointments
                .AsEnumerable()
                .Where(a => a.AppointmentDate.Date == targetDate
                            && (excludeId == null || a.Id != excludeId))
                .Select(a => a.AppointmentTime.ToString(@"hh\:mm"))
                .ToList();

            return Json(booked);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/Create
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/Create
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            ModelState.Remove(nameof(Appointment.Patient));

            if (ModelState.IsValid)
            {
                bool conflict = _context.Appointments
                    .AsEnumerable()
                    .Any(a => a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                              a.AppointmentTime == appointment.AppointmentTime);

                if (conflict)
                {
                    TempData["ErrorMessage"] = "That time slot is already booked. Please choose a different time.";
                    return RedirectToAction(nameof(Index));
                }

                appointment.CreatedAt = DateTime.Now;
                appointment.Status = "Pending";
                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Appointment para kang \"{appointment.PatientName}\" na-schedule na.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Adunay sayop sa form. Please check ang mga field.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/GetById/5
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult GetById(int id)
        {
            var apt = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);
            if (apt == null) return NotFound();

            return Json(new
            {
                apt.Id,
                apt.PatientId,
                apt.PatientName,
                apt.ContactNo,
                AppointmentDate = apt.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = apt.AppointmentTime.ToString(@"hh\:mm"),
                apt.ServiceType,
                apt.Notes,
                apt.Status,
                LinkedPatientName = apt.Patient?.FullName
            });
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/Details/5
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Details(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/Edit/5
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            if (appointment.Status == "Confirmed")
            {
                TempData["ErrorMessage"] = "Confirmed appointments can no longer be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/Edit/5
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Appointment appointment,
            string? NewPatientAddress, DateTime? NewPatientDOB, int? NewPatientAge,
            string? NewPatientMaritalStatus, string? NewPatientReligion, string? NewPatientOccupation,
            DateTime? NewPatientLMP, string? NewPatientAOG, DateTime? NewPatientEDC,
            string? NewPatientMenarche, string? NewPatientGravida, string? NewPatientTFAL)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id != appointment.Id)
            {
                if (isAjax) return Json(new { success = false, message = "Invalid appointment ID." });
                return NotFound();
            }

            ModelState.Remove(nameof(Appointment.Patient));

            if (ModelState.IsValid)
            {
                var existing = _context.Appointments.FirstOrDefault(a => a.Id == id);
                if (existing == null)
                {
                    if (isAjax) return Json(new { success = false, message = "Appointment not found." });
                    return NotFound();
                }

                // A confirmed booking is final. This server-side check prevents
                // edits even if someone bypasses the hidden Edit button.
                if (existing.Status == "Confirmed")
                {
                    const string confirmedError = "Confirmed appointments can no longer be edited.";
                    if (isAjax) return Json(new { success = false, message = confirmedError });
                    TempData["ErrorMessage"] = confirmedError;
                    return RedirectToAction(nameof(Index));
                }

                // -----------------------------------------------------------------
                // PROBLEM 1: "Rescheduled" requires BOTH a new date AND a new time.
                // Compare against the ORIGINAL saved date/time — if either is
                // unchanged, do NOT save and do NOT change the status.
                // -----------------------------------------------------------------
                bool reschedDateChanged = existing.AppointmentDate.Date != appointment.AppointmentDate.Date;
                bool reschedTimeChanged = existing.AppointmentTime != appointment.AppointmentTime;

                if (appointment.Status == "Rescheduled" && (!reschedDateChanged || !reschedTimeChanged))
                {
                    const string reschedError = "Please change both the appointment date and time before rescheduling.";
                    if (isAjax) return Json(new { success = false, message = reschedError });
                    TempData["ErrorMessage"] = reschedError;
                    return RedirectToAction(nameof(Index));
                }

                // Skip conflict check for Cancelled appointments
                if (appointment.Status != "Cancelled")
                {
                    bool conflict = _context.Appointments
                        .AsEnumerable()
                        .Any(a => a.Id != id &&
                                  a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                                  a.AppointmentTime == appointment.AppointmentTime);

                    if (conflict)
                    {
                        if (isAjax) return Json(new { success = false, message = "That time slot is already booked. Please choose a different time." });
                        TempData["ErrorMessage"] = "That time slot is already booked. Please choose a different time.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // -----------------------------------------------------------------
                // PROBLEMS 2 / 4 / 5: patient verification/creation happens FIRST.
                // The appointment status is only assigned AFTER a patient has been
                // verified against the Patients table (or successfully created),
                // so the DB can never show "Confirmed" when validation failed.
                // -----------------------------------------------------------------
                if (appointment.Status == "Confirmed")
                {
                    // Already linked to a real, registered Patient (e.g. scheduled
                    // via "Existing Appointment") — reuse it directly. Do not ask
                    // Existing/New Patient again or re-match by name.
                    var alreadyLinkedPatient = existing.PatientId.HasValue
                        ? _context.Patients.FirstOrDefault(p => p.Id == existing.PatientId.Value)
                        : null;

                    if (alreadyLinkedPatient != null)
                    {
                        existing.PatientName = alreadyLinkedPatient.FullName;
                        existing.ContactNo   = alreadyLinkedPatient.ContactNo;
                    }
                    else
                    {
                        var editPatientMode = Request.Form["EditPatientMode"].ToString();

                        if (editPatientMode == "existing")
                        {
                            // Verify the patient REALLY exists in the Patients table
                            // (source of truth) — never trust only the hidden PatientId.
                            var foundPatient = _context.Patients
                                .FirstOrDefault(p => p.FullName.ToLower() == appointment.PatientName.ToLower());
                            if (foundPatient == null)
                            {
                                var errMsg = $"Patient \"{appointment.PatientName}\" does not exist in the Patients records. Please select an existing patient or register as a New Patient.";
                                if (isAjax) return Json(new { success = false, message = errMsg });
                                TempData["ErrorMessage"] = errMsg;
                                return RedirectToAction(nameof(Index));
                            }

                            existing.PatientId   = foundPatient.Id;
                            existing.PatientName = foundPatient.FullName;
                            existing.ContactNo   = foundPatient.ContactNo;
                        }
                        else if (editPatientMode == "new")
                        {
                            // PROBLEM 4: validate the New Patient form BEFORE creating
                            // anything. If invalid → no patient created, not confirmed.
                            if (string.IsNullOrWhiteSpace(NewPatientAddress) || !NewPatientDOB.HasValue)
                            {
                                const string newPatErr = "Please complete the required new patient information (Address and Date of Birth) before confirming.";
                                if (isAjax) return Json(new { success = false, message = newPatErr });
                                TempData["ErrorMessage"] = newPatErr;
                                return RedirectToAction(nameof(Index));
                            }

                            var newPatient = new Patient
                            {
                                FullName       = appointment.PatientName,
                                ContactNo      = appointment.ContactNo,
                                Address        = NewPatientAddress,
                                DateOfBirth    = NewPatientDOB.Value,
                                Age            = NewPatientAge ?? Patient.CalculateAge(NewPatientDOB.Value),
                                MaritalStatus  = NewPatientMaritalStatus ?? "Single",
                                Religion       = NewPatientReligion ?? "",
                                Occupation     = NewPatientOccupation ?? "",
                                LMP            = NewPatientLMP ?? DateTime.Today,
                                AOG            = NewPatientAOG ?? "",
                                EDC            = NewPatientEDC ?? DateTime.Today,
                                Menarche       = NewPatientMenarche ?? "",
                                Gravida        = NewPatientGravida ?? "",
                                TFAL           = NewPatientTFAL ?? ""
                            };
                            _context.Patients.Add(newPatient);

                            // Atomic: linking via navigation property makes EF insert the
                            // Patient and update the Appointment in ONE SaveChanges call
                            // (single implicit transaction) — both succeed or both fail.
                            existing.Patient    = newPatient;
                            existing.PatientName = newPatient.FullName;
                            existing.ContactNo  = newPatient.ContactNo;
                        }
                        else
                        {
                            // PROBLEM 5: no patient mode selected — NEVER confirm an
                            // appointment without a verified/created patient.
                            const string modeErr = "Please select either Existing Patient or New Patient before confirming the appointment.";
                            if (isAjax) return Json(new { success = false, message = modeErr });
                            TempData["ErrorMessage"] = modeErr;
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                else
                {
                    existing.PatientId   = appointment.PatientId;
                    existing.PatientName = appointment.PatientName;
                    existing.ContactNo   = appointment.ContactNo;
                }

                // Keep existing date/time if Cancelled (none submitted from form)
                if (appointment.Status != "Cancelled")
                {
                    existing.AppointmentDate = appointment.AppointmentDate;
                    existing.AppointmentTime = appointment.AppointmentTime;
                }

                // Status is set LAST — only reached when every validation above passed.
                existing.Status = appointment.Status;

                _context.SaveChanges();

                if (isAjax) return Json(new { success = true });

                TempData["SuccessMessage"] = $"Appointment ni \"{existing.PatientName}\" na-update na.";
                return RedirectToAction(nameof(Index));
            }

            if (isAjax) return Json(new { success = false, message = "Adunay sayop sa pag-update. Please check the fields." });

            TempData["ErrorMessage"] = "Adunay sayop sa pag-update. Please check ang mga field.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/UpdateStatus
        // Changes status (Pending → Cancelled / Rescheduled).
        // "Confirmed" is handled via ConfirmExisting or ConfirmNewPatient.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            var allowed = new[] { "Pending", "Cancelled", "Rescheduled" };
            if (!allowed.Contains(status)) return BadRequest();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/Reschedule
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reschedule(int id, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            appointment.AppointmentDate = appointmentDate.Date;
            appointment.AppointmentTime = appointmentTime;
            appointment.Status = "Rescheduled";
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/ConfirmExisting
        // Links an existing Patient to the appointment and sets status "Confirmed".
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmExisting(int appointmentId, int patientId)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment == null) return NotFound();

            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            appointment.PatientId   = patient.Id;
            appointment.PatientName = patient.FullName;
            appointment.ContactNo   = patient.ContactNo;
            appointment.Status      = "Confirmed";
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Appointment ni \"{patient.FullName}\" na-confirm na.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/ConfirmNewPatient
        // Creates a new Patient and confirms the appointment.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmNewPatient(int appointmentId, Patient patient)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment == null)
            {
                if (isAjax) return Json(new { success = false, message = "Appointment not found." });
                return NotFound();
            }

            ModelState.Remove(nameof(Patient.Age));
            if (patient.DateOfBirth.HasValue)
                patient.Age = Patient.CalculateAge(patient.DateOfBirth.Value);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                var msg = !string.IsNullOrEmpty(errors) ? errors : "Please complete all required patient fields.";
                if (isAjax) return Json(new { success = false, message = msg });

                TempData["ErrorMessage"] = msg;
                return RedirectToAction(nameof(Index));
            }

            // -----------------------------------------------------------------
            // PROBLEM 4: patient creation + appointment confirmation must succeed
            // or fail together — wrap both SaveChanges calls in one transaction.
            // If anything fails, rollback leaves the appointment untouched.
            // -----------------------------------------------------------------
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Patients.Add(patient);
                _context.SaveChanges();

                appointment.PatientId   = patient.Id;
                appointment.PatientName = patient.FullName;
                appointment.ContactNo   = patient.ContactNo;
                appointment.Status      = "Confirmed";
                _context.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();

                const string dbErr = "An error occurred while saving. The patient was not registered and the appointment was not confirmed.";
                if (isAjax) return Json(new { success = false, message = dbErr });
                TempData["ErrorMessage"] = dbErr;
                return RedirectToAction(nameof(Index));
            }

            if (isAjax) return Json(new { success = true });

            TempData["SuccessMessage"] = $"Bag-ong pasyente nga \"{patient.FullName}\" na-register ug ang appointment na-confirm na.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // GET /Appointments/Delete/5
        // -----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/Delete/5
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string? confirmation)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            string name = appointment.PatientName;
            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Appointment para kang \"{name}\" na-delete na.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/ToggleInProcess/5
        // Only allowed when status = "Confirmed".
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleInProcess(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null) return NotFound();

            if (appointment.Status != "Confirmed")
            {
                TempData["ErrorMessage"] = "In Process is only available for Confirmed appointments.";
                return RedirectToAction(nameof(Index));
            }

            appointment.InProcess = !appointment.InProcess;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
