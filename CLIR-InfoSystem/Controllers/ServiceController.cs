using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class ServiceController : Controller
    {
        private readonly LibraryDbContext _context;

        public ServiceController(LibraryDbContext context)
        {
            _context = context;
        }

        // --- ODDS MANAGEMENT ---

        public IActionResult ManageODDS()
        {
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book) // Added to show which book is being requested
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View(odds);
        }

        public IActionResult ManageServices()
        {
            //check if expired
            var today = DateTime.Now;
            var expiredRequest = _context.Services
                .Where(b => b.RequestStatus == "Approved" && today > b.RequestDate.AddDays(7))
                .ToList();

            if (expiredRequest.Any())
            {
                foreach (var service in expiredRequest)
                {
                    service.RequestStatus = "Expired";
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

            return RedirectToAction("ManageServices");
        }
    }
}