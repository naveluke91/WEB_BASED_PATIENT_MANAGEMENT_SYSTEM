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
        private static readonly IReadOnlyDictionary<string, decimal> ServicePrices =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["Normal Delivery Fee & Newborn Care Package"] = 15000m,
                ["Prenatal"] = 100m,
                ["Implant"] = 0m,
                ["Implant Removal"] = 700m,
                ["DEPO"] = 200m,
                ["NORIFAM"] = 410m,
                ["IUD Insertion"] = 0m,
                ["IUD Removal"] = 600m,
                ["Anti-Tetanus Injection"] = 200m
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
        public IActionResult Create(int patientId, string? serviceName)
        {
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

            service.PatientId = patientId;
            service.ServiceName = canonicalServiceName;
            service.Price = price;
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
                _context.Services.Remove(service);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Service registration deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

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
