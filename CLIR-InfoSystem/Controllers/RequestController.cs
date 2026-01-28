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
        public IActionResult SubmitServiceRequest(ServiceRequest model) // Change this to accept the model
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            // Map the details
            model.PatronId = patronId;
            model.RequestDate = DateTime.Now.Date;
            model.RequestStatus = "Pending";

            ModelState.Remove("Patron"); // Keep this to avoid validation errors

            if (ModelState.IsValid)
            {
                _context.Services.Add(model); // 'model' now contains the ServiceType from the hidden input
                _context.SaveChanges();

                // If using AJAX, return Ok instead of Redirect
                return Ok(new { message = "Success" });
            }

            return BadRequest();
        }

        public IActionResult RequestTurnitin() => CheckExistingRequest("Turnitin");
        public IActionResult RequestGrammarly() => CheckExistingRequest("Grammarly");

        private IActionResult CheckExistingRequest(string type)
        {
            string? patronId = HttpContext.Session.GetString("UserId");

            // Check if request is Pending or Approved
            var existing = _context.Services.FirstOrDefault(s =>
                s.PatronId == patronId &&
                s.ServiceType == type &&
                (s.RequestStatus == "Pending" || s.RequestStatus == "Approved"));

            ViewBag.AlreadyRequested = existing != null;

            var model = new ServiceRequest { ServiceType = type };
            return View("ServiceForm", model);
        }


        public IActionResult Odds(bool success = false)
        {
            if (success) ViewBag.AlreadyRequested = true;

            // Fetch books to populate the dropdown
            ViewBag.Books = _context.Books
                .Select(b => new { b.AccessionId, b.Title })
                .ToList();

            var model = new OddsRequest
            {
                PatronId = HttpContext.Session.GetString("UserId")
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult SubmitOdds(OddsRequest model)
        {
            ModelState.Clear();
            var userId = HttpContext.Session.GetString("UserId");

            // 1. Limit: Max 5 requests per day
            var todayCount = _context.Odds.Count(o => o.PatronId == userId && o.RequestDate >= DateTime.Today);
            if (todayCount >= 5)
            {
                ModelState.AddModelError("", "You have reached the limit of 5 requests for today.");
                return View("Odds", model);
            }

            // 2. Uniqueness: Prevent duplicate requests for the same AccessionId
            if (!string.IsNullOrEmpty(model.AccessionId))
            {
                bool exists = _context.Odds.Any(o => o.PatronId == userId &&
                                                    o.AccessionId == model.AccessionId &&
                                                    o.RequestStatus == "Pending");
                if (exists)
                {
                    ModelState.AddModelError("", "You already have a pending request for this specific item.");
                    return View("Odds", model);
                }
            }

            // 3. Set Background Data
            model.RequestId = 0;
            model.PatronId = userId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            try
            {
                _context.Odds.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Odds", new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Database Error: " + ex.Message);
                return View("Odds", model);
            }
        }




    }


}