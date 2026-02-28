using CLIR_InfoSystem.Models;
using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class RequestController : BaseController
    {
        public RequestController(LibraryDbContext context) : base(context) { }

        #region Grammarly & Turnitin Services

        public IActionResult ServiceRequest(string? type)
        {

            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            var model = new ServiceRequest { PatronId = patronId };

            if (!string.IsNullOrEmpty(type))
                model.ServiceType = type;

            // Check if there is already a pending/approved request for the type
            ViewBag.AlreadyRequested = !string.IsNullOrEmpty(type) &&
                _context.Services.Any(s =>
                    s.PatronId == patronId &&
                    s.ServiceType == type &&
                    (s.RequestStatus == "Pending" || s.RequestStatus == "Approved"));

            return View("~/Views/Patron/PatronServiceRequest.cshtml", model);
        }


        [HttpPost]
        public IActionResult SubmitServiceRequest(ServiceRequest model)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return BadRequest("Session expired");

            model.PatronId = patronId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";
            model.StaffId = null;

            // Remove irrelevant ModelState keys
            ModelState.Remove("Patron");
            ModelState.Remove("Staff");
            ModelState.Remove("RequestStatus");

            // **SERVER-SIDE DUPLICATE CHECK**
            bool alreadyRequested = _context.Services.Any(s =>
                s.PatronId == patronId &&
                s.ServiceType == model.ServiceType &&
                (s.RequestStatus == "Pending" || s.RequestStatus == "Approved"));

            if (alreadyRequested)
            {
                return BadRequest("You already have a pending or approved request for this service.");
            }

            if (ModelState.IsValid)
            {
                _context.Services.Add(model);

                // AUDIT LOG
                LogAction($"Requested {model.ServiceType} account access", "Services");

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

            int todayCount = _context.Odds.Count(o =>
                o.PatronId == userId &&
                o.RequestDate >= DateTime.Today);

            ViewBag.TodayCount = todayCount;
            ViewBag.MaxLimit = 5;

            PopulateBooksDropdown();

            var model = new OddsRequest { PatronId = userId };
            return View("~/Views/Patron/PatronOdds.cshtml",model);
        }

        [HttpPost]
        public IActionResult SubmitOdds(OddsRequest model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return BadRequest("Session expired");

            // 1. Quota Check
            var countToday = _context.Odds.Count(o => o.PatronId == userId && o.RequestDate >= DateTime.Today);
            if (countToday >= 5)
                return BadRequest("Daily limit reached. You can only request 5 items per day.");

            // 2. URL VALIDATION
            if (!string.IsNullOrEmpty(model.ResourceLink))
            {
                bool isValidUrl = Uri.TryCreate(model.ResourceLink, UriKind.Absolute, out Uri? uriResult)
                                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!isValidUrl) return BadRequest("Please provide a valid URL link.");
            }

            // 3. DATE NEEDED VALIDATION 
            if (model.DateNeeded.HasValue && model.DateNeeded.Value.Date < DateTime.Today)
            {
                return BadRequest("The 'Date Needed' cannot be in the past.");
            }

            // 4. Duplicate Check
            if (!string.IsNullOrEmpty(model.AccessionId))
            {
                bool isDuplicate = _context.Odds.Any(o =>
                    o.PatronId == userId &&
                    o.AccessionId == model.AccessionId &&
                    o.RequestStatus == "Pending");

                if (isDuplicate) return BadRequest("You already have a pending request for this item.");
            }

            // Set System Fields
            model.PatronId = userId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            // Clean up navigation properties for validation
            ModelState.Remove("Patron");
            ModelState.Remove("Book");
            ModelState.Remove("Staff");

            if (ModelState.IsValid)
            {
                _context.Odds.Add(model);
                LogAction($"Submitted ODDS request for {model.MaterialType}. Needed by: {model.DateNeeded?.ToShortDateString()}", "ODDS");
                _context.SaveChanges();
                return Ok(new { message = "Success" });
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