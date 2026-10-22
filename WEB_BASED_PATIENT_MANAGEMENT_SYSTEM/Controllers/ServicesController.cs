using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers
{
    /// <summary>
    /// Registers the service selected by an existing patient.
    /// Clinical forms are handled separately by the Consultation module.
    /// </summary>
    public class ServicesController : Controller
    {
        public const decimal ImplantPrice = 700m;
        public const decimal IudPrice = 600m;

        private static readonly IReadOnlyDictionary<string, decimal> ServicePrices =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["Normal Delivery Fee & Newborn Care Package"] = 15000m,
                ["Prenatal"] = 100m,
                ["Implant"] = ImplantPrice,
                ["Implant Removal"] = ImplantPrice,
                ["DEPO"] = 200m,
                ["NORIFAM"] = 410m,
                ["IUD Insertion"] = IudPrice,
                ["IUD Removal"] = IudPrice,
                ["Anti-Tetanus Injection"] = 200m,

                // ==== [ST-SERVICE-9] START — Bag-ong service: Pap Smear (₱300) ====
                // Naa pa sab sa: ConsultationController.AvailableServices, _RegisterServiceModal,
                // _ServiceActionModals ug Consultation/Index.cshtml (parehas nga #9).
                /*
                ["Pap Smear"] = 300m,
                */
                // ==== [ST-SERVICE-9] END ====
            };

        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays the registered services. The patient's name is read through
        // Service.PatientId -> Patient.Id, rather than copied into Service.
        public IActionResult Index()
        {
            ViewBag.Patients = _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.FullName)
                .ToList();

            var services = _context.Services
                .AsNoTracking()
                .Include(s => s.Patient)
                .OrderByDescending(s => s.Id)
                .ToList();

            // ==== [ST-SERVICE-5] START — Status sa matag service: Waiting / In Progress / Completed / Paid ====
            /*
            var stConsultations = _context.Consultations.AsNoTracking()
                .Select(c => new { c.Id, c.ServiceId, c.AppointmentId, c.Status })
                .ToList();
            var stPaidConsultationIds = _context.Payments.AsNoTracking()
                .Select(p => p.ConsultationId)
                .ToHashSet();

            var stServiceStatus = new Dictionary<int, string>();
            foreach (var service in services)
            {
                var consultation = stConsultations.FirstOrDefault(c => c.ServiceId == service.Id
                    || (service.AppointmentId.HasValue && c.AppointmentId == service.AppointmentId));

                stServiceStatus[service.Id] = consultation == null ? "Waiting"
                    : consultation.Status != "Completed" ? consultation.Status
                    : stPaidConsultationIds.Contains(consultation.Id) ? "Paid" : "Completed";
            }
            ViewBag.StServiceStatus = stServiceStatus;
            */
            // ==== [ST-SERVICE-5] END ====

            // ==== [ST-SERVICE-10] START — Ipadala ang presyo gikan sa ServicePrices ngadto sa page ====
            // Aron ang presyo sa mga tile kay gikan sa usa ra ka lugar (kini nga dictionary).
            /*
            ViewBag.StServicePrices = ServicePrices;
            */
            // ==== [ST-SERVICE-10] END ====

            return View(services);
        }

        [HttpGet]
        public IActionResult Create(int? patientId)
        {
            // Registration is intentionally done in the Services page modal.
            // This keeps the Services module limited to patient + service selection.
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int patientId, int? appointmentId, string? serviceName)
        {
            // ==== [ST-SERVICE-11] START — Daghang service sa usa ka registration (Walk-In ra) ====
            // Ang JS mo-send og "Prenatal|Anti-Tetanus Injection" (gibulag sa "|").
            /*
            if (!appointmentId.HasValue && serviceName != null && serviceName.Contains('|'))
            {
                if (!_context.Patients.Any(patient => patient.Id == patientId))
                {
                    TempData["ErrorMessage"] = "Please select a registered patient.";
                    return RedirectToAction(nameof(Index));
                }

                if (AvailableAppointments(patientId).Any())
                {
                    TempData["ErrorMessage"] = "This patient has a confirmed appointment. Please register the service for that appointment.";
                    return RedirectToAction(nameof(Index));
                }

                var stNewServices = new List<Service>();
                foreach (var name in serviceName.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct())
                {
                    if (!TryGetService(name, out var stName, out var stPrice))
                    {
                        TempData["ErrorMessage"] = "Please select a valid service.";
                        return RedirectToAction(nameof(Index));
                    }
                    stNewServices.Add(new Service { PatientId = patientId, ServiceName = stName, Price = stPrice });
                }

                _context.Services.AddRange(stNewServices);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"{stNewServices.Count} services registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            */
            // ==== [ST-SERVICE-11] END ====

            if (!TryGetService(serviceName, out var canonicalServiceName, out var price))
            {
                TempData["ErrorMessage"] = "Please select a valid service.";
                return RedirectToAction(nameof(Index));
            }

            if (!_context.Patients.Any(patient => patient.Id == patientId))
            {
                TempData["ErrorMessage"] = "Please select a registered patient.";
                return RedirectToAction(nameof(Index));
            }

            // Walk-In: no appointment was selected, so no appointment-specific
            // validation applies. Save the service against the patient only.
            if (!appointmentId.HasValue)
            {
                if (AvailableAppointments(patientId).Any())
                {
                    TempData["ErrorMessage"] = "This patient has a confirmed appointment. Please register the service for that appointment.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Services.Add(new Service
                {
                    PatientId = patientId,
                    ServiceName = canonicalServiceName,
                    Price = price
                });
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Service registered successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Appointment-based: a service belongs to one specific appointment.
            // Do not look up a service by PatientId here because one patient can
            // have more than one appointment with different services.
            var appointment = _context.Appointments.FirstOrDefault(appointment =>
                appointment.Id == appointmentId.Value &&
                appointment.PatientId == patientId &&
                appointment.Status == "Confirmed" &&
                !appointment.InProcess &&
                string.IsNullOrWhiteSpace(appointment.ServiceType) &&
                !_context.Services.Any(service => service.AppointmentId == appointment.Id) &&
                !_context.Consultations.Any(consultation => consultation.AppointmentId == appointment.Id));

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "That appointment is no longer available. Please choose an unassigned confirmed appointment.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Services.Add(new Service
                {
                    PatientId = patientId,
                    ServiceName = canonicalServiceName,
                    Price = price,
                    AppointmentId = appointment.Id
                });

                // Consultation reads Appointment.ServiceType, so save the
                // selected service on this exact appointment in the same unit
                // of work as the service record.
                appointment.ServiceType = canonicalServiceName;
                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "The service could not be registered. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Service registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetAvailableAppointments(int patientId)
        {
            var appointments = AvailableAppointments(patientId)
                .AsNoTracking()
                .OrderBy(appointment => appointment.AppointmentDate)
                .ThenBy(appointment => appointment.AppointmentTime)
                .Select(appointment => new
                {
                    appointment.Id,
                    appointment.AppointmentDate,
                    appointment.AppointmentTime
                })
                .AsEnumerable()
                .Select(appointment => new
                {
                    id = appointment.Id,
                    label = $"{appointment.AppointmentDate:MMMM dd, yyyy} · {DateTime.Today.Add(appointment.AppointmentTime):h:mm tt}"
                })
                .ToList();

            return Json(appointments);
        }

        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var service = _context.Services
                .AsNoTracking()
                .Include(s => s.Patient)
                .FirstOrDefault(s => s.Id == id.Value);

            return service == null ? NotFound() : View(service);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var service = _context.Services
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == id.Value);

            if (service == null)
            {
                return NotFound();
            }

            ViewBag.Patients = _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.FullName)
                .ToList();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, int patientId, string? serviceName)
        {
            var service = _context.Services.Find(id);
            if (service == null)
            {
                return NotFound();
            }

            // Dili na usbon kung nagsugod na ang konsultasyon niini.
            if (HasConsultation(service))
            {
                TempData["ErrorMessage"] = "This service can no longer be edited because its consultation has started.";
                return RedirectToAction(nameof(Index));
            }

            if (!TryGetService(serviceName, out var canonicalServiceName, out var price))
            {
                TempData["ErrorMessage"] = "Please select a valid service.";
                return RedirectToAction(nameof(Index));
            }

            if (!_context.Patients.Any(patient => patient.Id == patientId))
            {
                TempData["ErrorMessage"] = "Please select a registered patient.";
                return RedirectToAction(nameof(Index));
            }

            // Ang serbisyo sa appointment dili ibalhin sa laing pasyente.
            if (service.AppointmentId.HasValue && patientId != service.PatientId)
            {
                TempData["ErrorMessage"] = "A service linked to an appointment cannot be moved to another patient.";
                return RedirectToAction(nameof(Index));
            }

            service.PatientId = patientId;
            service.ServiceName = canonicalServiceName;
            service.Price = price;

            // I-sync ang ServiceType sa appointment niini nga serbisyo.
            if (service.AppointmentId.HasValue)
            {
                var appointment = _context.Appointments.Find(service.AppointmentId.Value);
                if (appointment != null)
                    appointment.ServiceType = canonicalServiceName;
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Service registration updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var service = _context.Services
                .AsNoTracking()
                .Include(s => s.Patient)
                .FirstOrDefault(s => s.Id == id.Value);

            return service == null ? NotFound() : View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var service = _context.Services.Find(id);
            if (service != null)
            {
                // Dili i-delete kung nagsugod na ang konsultasyon niini.
                if (HasConsultation(service))
                {
                    TempData["ErrorMessage"] = "This service can no longer be deleted because its consultation has started.";
                    return RedirectToAction(nameof(Index));
                }

                // Ibalik ang appointment nga walay serbisyo.
                if (service.AppointmentId.HasValue)
                {
                    var appointment = _context.Appointments.Find(service.AppointmentId.Value);
                    if (appointment != null)
                        appointment.ServiceType = null;
                }

                _context.Services.Remove(service);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Service registration deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Naa bay konsultasyon gikan niini nga serbisyo (walk-in o appointment)?
        private bool HasConsultation(Service service) =>
            _context.Consultations.Any(c => c.ServiceId == service.Id
                || (service.AppointmentId.HasValue && c.AppointmentId == service.AppointmentId));

        private IQueryable<Appointment> AvailableAppointments(int patientId) =>
            _context.Appointments.Where(appointment =>
                appointment.PatientId == patientId &&
                appointment.Status == "Confirmed" &&
                !appointment.InProcess &&
                string.IsNullOrWhiteSpace(appointment.ServiceType) &&
                !_context.Consultations.Any(consultation => consultation.AppointmentId == appointment.Id));

        private static bool TryGetService(string? requestedServiceName, out string serviceName, out decimal price)
        {
            serviceName = string.Empty;
            price = 0m;

            if (string.IsNullOrWhiteSpace(requestedServiceName))
            {
                return false;
            }

            var requestedName = requestedServiceName.Trim();
            var matchingName = ServicePrices.Keys.FirstOrDefault(name =>
                string.Equals(name, requestedName, StringComparison.OrdinalIgnoreCase));

            if (matchingName == null)
            {
                return false;
            }

            serviceName = matchingName;
            price = ServicePrices[matchingName];
            return true;
        }
    }
}
