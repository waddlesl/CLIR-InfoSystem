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

        public IActionResult Services() => View();

        [HttpPost]
        public IActionResult SubmitServiceRequest(ServiceRequest model)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            model.PatronId = patronId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            ModelState.Remove("Patron");

            if (ModelState.IsValid)
            {
                _context.Services.Add(model);
                _context.SaveChanges();
                return Ok(new { message = "Success" });
            }

            return BadRequest();
        }

        public IActionResult RequestTurnitin() => CheckExistingRequest("Turnitin");
        public IActionResult RequestGrammarly() => CheckExistingRequest("Grammarly");

        private IActionResult CheckExistingRequest(string type)
        {
            string? patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

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
            if (success) ViewBag.SuccessMessage = "Request submitted successfully!";

            PopulateBooksDropdown();

            var model = new OddsRequest
            {
                PatronId = HttpContext.Session.GetString("UserId")
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult SubmitOdds(OddsRequest model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // 1. Limit: Max 5 requests per day
            var todayCount = _context.Odds.Count(o => o.PatronId == userId && o.RequestDate >= DateTime.Today);
            if (todayCount >= 5)
            {
                ModelState.AddModelError("", "You have reached the limit of 5 requests for today.");
                PopulateBooksDropdown();
                return View("Odds", model);
            }

            // 2. Uniqueness: Prevent duplicate pending requests for the same AccessionId
            if (!string.IsNullOrEmpty(model.AccessionId))
            {
                bool exists = _context.Odds.Any(o => o.PatronId == userId &&
                                                    o.AccessionId == model.AccessionId &&
                                                    o.RequestStatus == "Pending");
                if (exists)
                {
                    ModelState.AddModelError("", "You already have a pending request for this specific item.");
                    PopulateBooksDropdown();
                    return View("Odds", model);
                }
            }

            model.PatronId = userId;
            model.RequestDate = DateTime.Now;
            model.RequestStatus = "Pending";

            ModelState.Remove("Patron");
            ModelState.Remove("Book");

            if (ModelState.IsValid)
            {
                _context.Odds.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Odds", new { success = true });
            }

            PopulateBooksDropdown();
            return View("Odds", model);
        }

        private void PopulateBooksDropdown()
        {
            ViewBag.Books = _context.Books
                .Select(b => new { b.AccessionId, b.Title })
                .ToList();
        }
    }
}