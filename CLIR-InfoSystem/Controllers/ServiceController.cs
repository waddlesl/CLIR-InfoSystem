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
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffManageODDS.cshtml", odds);
        }
        
        public IActionResult ODDSHistory()
        {
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffODDSHistory.cshtml", odds);
        }

        public IActionResult ManageServices()
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
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

            return View("~/Views/Staff/StaffManageServices.cshtml", services);
        }

        public IActionResult ServicesHistory()
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

            return View("~/Views/Staff/StaffServicesHistory.cshtml", services);
        }


        [HttpGet]
        public IActionResult UpdateOddsStatus(int id, string status)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.Odds.Find(id);
            if(request == null)
            {
                TempData["Error"] = "Service request not found.";
                return RedirectToAction("ManageODDS"); 
            }


            request.RequestStatus = status;
            if (status == "Fulfilled")
            {
                request.DateOfAccessProvided = DateTime.Now;
            }

            // AUDIT LOG
            LogAction($"Updated ODDS Request #{id} status to: {status}", "System");
            _context.SaveChanges();
            return RedirectToAction("ManageODDS");
        }

        // --- GRAMMARLY & TURNITIN MANAGEMENT ---

        public IActionResult ManageServiceRequests()
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var requests = _context.Services
                .Include(s => s.Patron)
                .OrderByDescending(s => s.RequestDate)
                .ToList();

            return View(requests);
        }

        [HttpGet]
        public IActionResult UpdateServiceStatus(int requestId, string status)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.Services.Find(requestId);
            if (request == null) return NotFound();

            request.RequestStatus = status;


            // AUDIT LOG
            LogAction($"Updated Service Request #{requestId} ({request.ServiceType}) status to: {status}", "Services");
            _context.SaveChanges();
            return RedirectToAction("ManageServices");
        }
    }
}