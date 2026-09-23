using System.Security.Claims;
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
            // I-verify ang PatientId gikan sa form sa Patients table.
            Patient? linkedPatient = null;
            if (appointment.PatientId.HasValue)
            {
                linkedPatient = _context.Patients.FirstOrDefault(p => p.Id == appointment.PatientId.Value);
                if (linkedPatient == null)
                {
                    TempData["ErrorMessage"] = "Wala makit-an ang pasyente. Pilia pag-usab gikan sa listahan.";
                    return RedirectToAction(nameof(Index));
                }
                appointment.PatientName = linkedPatient.FullName;
            }

            // I-validate pag-usab sa server (ngalan, contact, petsa, oras).
            var errors = ValidateAppointmentInput(appointment, checkName: linkedPatient == null, checkSchedule: true);

            if (errors.Count == 0)
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

                // Dili pagsaligan ang ubang field gikan sa form.
                appointment.Id = 0;
                appointment.Patient = null;
                appointment.ServiceType = null;
                appointment.InProcess = false;
                appointment.CreatedAt = DateTime.Now;
                appointment.Status = "Pending";
                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Appointment para kang \"{appointment.PatientName}\" na-schedule na.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = errors.Values.First();
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

                // I-validate pag-usab sa server; ang Cancelled dili na i-check ang petsa/oras.
                bool isCancelled = appointment.Status == "Cancelled";
                var errors = ValidateAppointmentInput(appointment, checkName: true, checkSchedule: !isCancelled);
                if (!AllowedStatuses.Contains(appointment.Status))
                    errors.TryAdd(nameof(Appointment.Status), "Pilia ang valid nga status.");

                // -----------------------------------------------------------------
                // PROBLEM 1: "Rescheduled" requires a new date OR a new time (or both).
                // Compare values against the ORIGINAL saved record: the date part
                // only, and the time to the minute (the form posts "HH:mm"). Only
                // an unchanged date AND an unchanged time is rejected — then
                // nothing is saved and the status does not change.
                // -----------------------------------------------------------------
                bool reschedDateChanged = existing.AppointmentDate.Date != appointment.AppointmentDate.Date;
                bool reschedTimeChanged = TruncateToMinute(existing.AppointmentTime) != TruncateToMinute(appointment.AppointmentTime);

                if (errors.Count == 0 && appointment.Status == "Rescheduled" && !reschedDateChanged && !reschedTimeChanged)
                {
                    errors[nameof(Appointment.AppointmentDate)] = "Usba ang petsa o oras.";
                    errors[nameof(Appointment.AppointmentTime)] = "Usba ang petsa o oras.";
                }

                // Skip conflict check for Cancelled appointments
                if (errors.Count == 0 && !isCancelled)
                {
                    bool conflict = _context.Appointments
                        .AsEnumerable()
                        .Any(a => a.Id != id &&
                                  a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                                  a.AppointmentTime == appointment.AppointmentTime);

                    if (conflict)
                        errors[nameof(Appointment.AppointmentTime)] = "Nagamit na kini nga schedule.";
                }

                if (errors.Count > 0)
                    return AppointmentValidationFailure(errors, isAjax);

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
                            // Kung daghan og parehas nga ngalan, gamita ang contact number.
                            var nameMatches = _context.Patients
                                .Where(p => p.FullName.ToLower() == appointment.PatientName.ToLower())
                                .ToList();
                            var candidates = nameMatches.Count > 1
                                ? nameMatches.Where(p => p.ContactNo == appointment.ContactNo).ToList()
                                : nameMatches;
                            var foundPatient = candidates.Count == 1 ? candidates[0] : null;
                            if (foundPatient == null)
                            {
                                var errMsg = nameMatches.Count > 1
                                    ? "Daghan og pasyente nga parehas og ngalan; susiha ang contact number."
                                    : $"Patient \"{appointment.PatientName}\" does not exist in the Patients records. Please select an existing patient or register as a New Patient.";
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
                            // Parehas nga rule sa Add Patient; ang edad gikan sa DOB.
                            var newPatient = new Patient
                            {
                                FullName       = appointment.PatientName,
                                ContactNo      = appointment.ContactNo,
                                Address        = NewPatientAddress ?? "",
                                DateOfBirth    = NewPatientDOB,
                                MaritalStatus  = NewPatientMaritalStatus ?? "",
                                Religion       = NewPatientReligion,
                                Occupation     = NewPatientOccupation,
                                LMP            = NewPatientLMP,
                                AOG            = NewPatientAOG,
                                EDC            = NewPatientEDC,
                                Menarche       = NewPatientMenarche,
                                Gravida        = NewPatientGravida,
                                TFAL           = NewPatientTFAL
                            };
                            newPatient.TrimTextFields();

                            var newPatientErrors = Patient.ValidateInput(newPatient);
                            if (newPatientErrors.Count > 0)
                            {
                                var newPatErr = "Kulang o sayop ang datos sa bag-ong pasyente: " + newPatientErrors.Values.First();
                                if (isAjax) return Json(new { success = false, message = newPatErr });
                                TempData["ErrorMessage"] = newPatErr;
                                return RedirectToAction(nameof(Index));
                            }

                            newPatient.Age = Patient.CalculateAge(newPatient.DateOfBirth!.Value);
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
                    // Dili usbon ang PatientId gikan sa hidden field sa form.
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

            // Dili na usbon ang confirmed; ang Rescheduled kinahanglan og bag-ong petsa o oras.
            if (appointment.Status == "Confirmed" || status == "Rescheduled")
            {
                TempData["ErrorMessage"] = appointment.Status == "Confirmed"
                    ? "Confirmed appointments can no longer be edited."
                    : "Usba ang petsa o oras una i-reschedule.";
                return RedirectToAction(nameof(Index));
            }

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

            // I-validate ang bag-ong schedule sama sa Edit.
            string? scheduleError = null;
            if (appointment.Status == "Confirmed")
                scheduleError = "Confirmed appointments can no longer be edited.";
            else if (appointmentDate.Date < DateTime.Today.AddDays(1))
                scheduleError = "Pilia ang petsa sugod ugma.";
            else if (!AllowedTimeSlots.Contains(appointmentTime))
                scheduleError = "Pilia ang valid nga oras.";
            else if (appointment.AppointmentDate.Date == appointmentDate.Date && appointment.AppointmentTime == appointmentTime)
                scheduleError = "Usba ang petsa o oras una i-reschedule.";
            else if (_context.Appointments.AsEnumerable().Any(a => a.Id != id
                         && a.AppointmentDate.Date == appointmentDate.Date
                         && a.AppointmentTime == appointmentTime))
                scheduleError = "Nagamit na kini nga schedule.";

            if (scheduleError != null)
            {
                TempData["ErrorMessage"] = scheduleError;
                return RedirectToAction(nameof(Index));
            }

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

            // Dili na i-confirm pag-usab.
            if (appointment.Status == "Confirmed")
            {
                TempData["ErrorMessage"] = "Na-confirm na kini nga appointment.";
                return RedirectToAction(nameof(Index));
            }

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
        public IActionResult ConfirmNewPatient(int appointmentId, [Bind(Patient.FormFields)] Patient patient)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment == null)
            {
                if (isAjax) return Json(new { success = false, message = "Appointment not found." });
                return NotFound();
            }

            // Dili na pwede kung naa nay pasyente o confirmed na.
            if (appointment.Status == "Confirmed" || appointment.PatientId.HasValue)
            {
                const string linkedErr = "Naa nay pasyente o confirmed na kini nga appointment.";
                if (isAjax) return Json(new { success = false, message = linkedErr });
                TempData["ErrorMessage"] = linkedErr;
                return RedirectToAction(nameof(Index));
            }

            // Parehas nga rule sa Add Patient; ang edad gikan sa DOB.
            patient.TrimTextFields();
            patient.Age = patient.DateOfBirth.HasValue ? Patient.CalculateAge(patient.DateOfBirth.Value) : 0;
            ModelState.Clear();

            var errors = Patient.ValidateInput(patient);
            if (errors.Count > 0)
            {
                var msg = errors.Values.First();
                if (isAjax) return Json(new { success = false, message = msg, errors });

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

            // An appointment that already started a consultation is kept. Deleting it
            // would unlink its Service (ON DELETE SET NULL), and Consultation would list
            // that service again as a Waiting walk-in to be consulted and billed twice.
            if (_context.Consultations.Any(c => c.AppointmentId == id))
            {
                TempData["ErrorMessage"] = "This appointment already has a consultation, so it can no longer be deleted.";
                return RedirectToAction(nameof(Index));
            }

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

            // Ang konsultasyon ra ang mo-usab sa InProcess kung naa na.
            if (_context.Consultations.Any(c => c.AppointmentId == id))
            {
                TempData["ErrorMessage"] = "Naa nay konsultasyon kini nga appointment.";
                return RedirectToAction(nameof(Index));
            }

            appointment.InProcess = !appointment.InProcess;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------------------------
        // POST /Appointments/MarkNotificationRead/5
        // Marks the reminder notification for this appointment as read for the
        // signed-in account only. The appointment itself is never changed.
        // -----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkNotificationRead(int id)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userAccountId))
                return Forbid();

            var alreadyRead = _context.NotificationReads
                .Any(r => r.UserAccountId == userAccountId && r.AppointmentId == id);
            if (!alreadyRead)
            {
                _context.NotificationReads.Add(new NotificationRead
                {
                    UserAccountId = userAccountId,
                    AppointmentId = id,
                    ReadAtUtc = DateTime.UtcNow
                });
                _context.SaveChanges();
            }

            return Ok();
        }

        // Appointment times are chosen in whole minutes ("HH:mm"), so the
        // reschedule check compares them at that precision.
        private static TimeSpan TruncateToMinute(TimeSpan time) =>
            new TimeSpan(time.Days, time.Hours, time.Minutes, 0);

        // Mga oras nga pwede (parehas sa ALL_SLOTS sa appointment.js).
        private static readonly TimeSpan[] AllowedTimeSlots =
        {
            new(9, 0, 0), new(10, 0, 0), new(11, 0, 0), new(13, 0, 0), new(14, 0, 0), new(15, 0, 0)
        };

        // Mga status nga gidawat sa Edit.
        private static readonly string[] AllowedStatuses = { "Pending", "Confirmed", "Cancelled", "Rescheduled" };

        // I-validate ang ngalan, contact, petsa ug oras; ibalik ang sayop matag field.
        private Dictionary<string, string> ValidateAppointmentInput(Appointment appointment, bool checkName, bool checkSchedule)
        {
            var errors = new Dictionary<string, string>();
            appointment.PatientName = appointment.PatientName?.Trim() ?? string.Empty;
            appointment.ContactNo = appointment.ContactNo?.Trim() ?? string.Empty;

            if (checkName)
            {
                if (appointment.PatientName.Length == 0)
                    errors[nameof(Appointment.PatientName)] = "Kinahanglan kini nga field.";
                else if (appointment.PatientName.Length > 150 || !Patient.IsValidPersonName(appointment.PatientName))
                    errors[nameof(Appointment.PatientName)] = "Dili valid ang ngalan.";
            }

            if (appointment.ContactNo.Length == 0)
                errors[nameof(Appointment.ContactNo)] = "Contact number is required.";
            else if (!Patient.IsValidContactNo(appointment.ContactNo))
                errors[nameof(Appointment.ContactNo)] = "Contact number must contain 11 digits.";

            if (checkSchedule)
            {
                // Sayop sa pag-bind = walay petsa o oras.
                if (HasBindingError(nameof(Appointment.AppointmentDate)) || appointment.AppointmentDate == default)
                    errors[nameof(Appointment.AppointmentDate)] = "Pilia ang petsa.";
                else if (appointment.AppointmentDate.Date < DateTime.Today.AddDays(1))
                    errors[nameof(Appointment.AppointmentDate)] = "Pilia ang petsa sugod ugma.";

                // Valid ra nga oras.
                if (HasBindingError(nameof(Appointment.AppointmentTime)) || !AllowedTimeSlots.Contains(appointment.AppointmentTime))
                    errors[nameof(Appointment.AppointmentTime)] = "Pilia ang valid nga oras.";
            }

            return errors;
        }

        private bool HasBindingError(string key) =>
            ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0;

        // Ibalik ang sayop matag field (AJAX) o sa TempData.
        private IActionResult AppointmentValidationFailure(Dictionary<string, string> errors, bool isAjax)
        {
            var message = errors.Values.First();
            if (isAjax) return Json(new { success = false, message, errors });

            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}
