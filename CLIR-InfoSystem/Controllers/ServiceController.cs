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
                .Include(s => s.Book) // Added to show which book is being requested
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View(odds);
        }

        [HttpPost]
        public IActionResult UpdateOddsStatus(int requestId, string status)
        {
            var request = _context.Odds.Find(requestId);
            if (request == null) return NotFound();

            request.RequestStatus = status;
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
        public IActionResult UpdateServiceStatus(int serviceId, string status)
        {
            var request = _context.Services.Find(serviceId);
            if (request == null) return NotFound();

            request.RequestStatus = status;
            _context.SaveChanges();

            return RedirectToAction("ManageServiceRequests");
        }
    }
}