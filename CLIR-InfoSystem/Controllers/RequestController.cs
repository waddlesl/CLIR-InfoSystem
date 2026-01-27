using CLIR_InfoSystem.Models;
using CLIR_InfoSystem.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLIR_InfoSystem.Controllers
{
    // 1. Inherit from Controller
    public class RequestController : Controller
    {
        private readonly LibraryDbContext _context;

        // 2. Inject the Database Context
        public RequestController(LibraryDbContext context)
        {
            _context = context;
        }

        // 3. GET Action to show the form
        public IActionResult Services()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitServiceRequest(string serviceType)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            var newRequest = new ServiceRequest
            {
                PatronId = patronId,
                RequestDate = DateTime.Now.Date,
                ServiceType = serviceType,
                RequestStatus = "Pending"
            };

            // Ignore navigation objects for validation
            ModelState.Remove("Patron");

            if (ModelState.IsValid)
            {
                _context.Services.Add(newRequest);
                _context.SaveChanges();
                TempData["Success"] = $"Your {serviceType} request has been submitted!";
                return RedirectToAction("Index", "Patron");
            }

            return View("Services");
        }

        // Shows the Turnitin Form
        public IActionResult RequestTurnitin() => View("ServiceForm", "Turnitin");

        // Shows the Grammarly Form
        public IActionResult RequestGrammarly() => View("ServiceForm", "Grammarly");
    }
}