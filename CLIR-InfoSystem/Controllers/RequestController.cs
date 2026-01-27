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
        public IActionResult SubmitServiceRequest(ServiceRequest model)
        {
            model.PatronId = HttpContext.Session.GetString("UserId");
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            _context.Services.Add(model);
            _context.SaveChanges();

            // Change this line to stay on the form
            // We pass the ServiceType back so the CheckExistingRequest logic runs
            if (model.ServiceType == "Grammarly")
            {
                return RedirectToAction("RequestGrammarly");
            }
            else
            {
                return RedirectToAction("RequestTurnitin");
            }
        }

        // Action for Turnitin
        public IActionResult RequestTurnitin()
        {
            return CheckExistingRequest("Turnitin");
        }

        // Action for Grammarly
        public IActionResult RequestGrammarly()
        {
            return CheckExistingRequest("Grammarly");
        }

        // Reusable check to see if they already requested the service
        private IActionResult CheckExistingRequest(string type)
        {
            var userId = HttpContext.Session.GetString("UserId");

            // Check if a pending request already exists in the database
            var existing = _context.Services.FirstOrDefault(s =>
                s.PatronId == userId &&
                s.ServiceType == type &&
                s.RequestStatus == "Pending");

            // Pass the existence status to the View
            ViewBag.IsAlreadySubmitted = (existing != null);

            return View("ServiceForm", new ServiceRequest { ServiceType = type });
        }



    }
}