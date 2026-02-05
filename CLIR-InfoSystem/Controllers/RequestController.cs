using CLIR_InfoSystem.Models;
using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class RequestController : Controller
    {
        private readonly LibraryDbContext _context;

        public RequestController(LibraryDbContext context)
        {
            _context = context;
        }

        #region Grammarly & Turnitin Services

        public IActionResult RequestTurnitin() => CheckExistingRequest("Turnitin");
        public IActionResult RequestGrammarly() => CheckExistingRequest("Grammarly");

        private IActionResult CheckExistingRequest(string type)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            // Check database for existing active requests
            var existing = _context.Services.FirstOrDefault(s =>
                s.PatronId == patronId &&
                s.ServiceType == type &&
                (s.RequestStatus == "Pending" || s.RequestStatus == "Approved"));

            ViewBag.AlreadyRequested = (existing != null);

            var model = new ServiceRequest
            {
                ServiceType = type,
                PatronId = patronId
            };

            return View("ServiceForm", model);
        }

        [HttpPost]
        public IActionResult SubmitServiceRequest(ServiceRequest model)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return BadRequest("Session expired");

            // Force System-set values
            model.PatronId = patronId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";
            model.StaffId = null;

            // Clear validation for navigation properties and staff
            ModelState.Remove("Patron");
            ModelState.Remove("Staff");
            ModelState.Remove("RequestStatus");

            if (ModelState.IsValid)
            {
                _context.Services.Add(model);
                _context.SaveChanges();
                return Ok(new { message = "Success" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(string.Join(", ", errors));
        }

        #endregion

        #region ODDS (Online Document Delivery Service)

        public IActionResult Odds(bool success = false)
        {
            string? userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // Calculate how many requests were made TODAY
            int todayCount = _context.Odds.Count(o =>
                o.PatronId == userId &&
                o.RequestDate >= DateTime.Today);

            ViewBag.TodayCount = todayCount; // Pass this to the view
            ViewBag.MaxLimit = 5;

            PopulateBooksDropdown();

            var model = new OddsRequest { PatronId = userId };
            return View(model);
        }

        [HttpPost]
        public IActionResult SubmitOdds(OddsRequest model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return BadRequest("Session expired");

            // 1. Daily Limit Check (Max 5)
            var countToday = _context.Odds.Count(o =>
                o.PatronId == userId &&
                o.RequestDate >= DateTime.Today);

            if (countToday >= 5)
                return BadRequest("Daily limit reached. You can only request 5 items per day.");

            // 2. Duplicate Check (Prevent same item if Pending)
            if (!string.IsNullOrEmpty(model.AccessionId))
            {
                bool isDuplicate = _context.Odds.Any(o =>
                    o.PatronId == userId &&
                    o.AccessionId == model.AccessionId &&
                    o.RequestStatus == "Pending");

                if (isDuplicate) return BadRequest("You already have a pending request for this specific item.");
            }

            // 3. Set System Fields
            model.PatronId = userId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            ModelState.Remove("Patron");
            ModelState.Remove("Book");
            ModelState.Remove("Staff");

            if (ModelState.IsValid)
            {
                _context.Odds.Add(model);
                _context.SaveChanges();
                return Ok(new { remaining = 4 - countToday }); // Return remaining quota
            }

            return BadRequest("Invalid form data.");
        }

        private void PopulateBooksDropdown()
        {
            ViewBag.Books = _context.Books
                .Select(b => new { b.AccessionId, b.Title })
                .ToList();
        }

        #endregion
    }
}