using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class ServiceController : BaseController
    {
        public ServiceController(LibraryDbContext context) : base(context)
        {
        }

        // --- ODDS MANAGEMENT ---

        public IActionResult ManageODDS()
        {
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View(odds);
        }

        public IActionResult ManageServices()
        {
            var today = DateTime.Now;
            var expiredRequest = _context.Services
                .Where(b => b.RequestStatus == "Approved" && today > b.RequestDate.AddDays(7))
                .ToList();

            if (expiredRequest.Any())
            {
                foreach (var service in expiredRequest)
                {
                    service.RequestStatus = "Expired";
                    // AUDIT LOG (Internal System Action)
                    LogAction($"Service Request #{service.ServiceId} automatically expired.", "System");
                }
                _context.SaveChanges();
            }

            var services = _context.Services
                .Include(s => s.Patron)
                    .ThenInclude(p => p.Department)
                .Include(s => s.Staff)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View(services);
        }

        [HttpPost]
        public IActionResult UpdateOddsStatus(int requestId, string status)
        {
            var request = _context.Odds.Find(requestId);
            if (request == null) return NotFound();

            request.RequestStatus = status;
            if (status == "Fulfilled")
            {
                request.DateOfAccessProvided = DateTime.Now;
            }

            _context.SaveChanges();

            // AUDIT LOG
            LogAction($"Updated ODDS Request #{requestId} status to: {status}", "Services");

            return RedirectToAction("ManageODDS");
        }

        // --- GRAMMARLY & TURNITIN MANAGEMENT ---

        public IActionResult ManageServiceRequests()
        {
            var requests = _context.Services
                .Include(s => s.Patron)
                .OrderByDescending(s => s.RequestDate)
                .ToList();

            return View(requests);
        }

        [HttpPost]
        public IActionResult UpdateServiceStatus(int requestId, string status)
        {
            var request = _context.Services.Find(requestId);
            if (request == null) return NotFound();

            request.RequestStatus = status;
            _context.SaveChanges();

            // AUDIT LOG
            LogAction($"Updated Service Request #{requestId} ({request.ServiceType}) status to: {status}", "Services");

            return RedirectToAction("ManageServices");
        }
    }
}