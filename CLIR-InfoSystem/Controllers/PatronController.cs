using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class PatronController : Controller
    {
        private readonly LibraryDbContext _context;
        public PatronController(LibraryDbContext context) => _context = context;

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // Fetch all related activities
            ViewBag.SeatHistory = _context.SeatBookings
                .Include(s => s.TimeSlot)
                .Include(s => s.LibrarySeat)
                .Where(s => s.PatronId == userId)
                .OrderByDescending(s => s.BookingDate)
                .ToList();

            ViewBag.OddsHistory = _context.Odds
                .Where(o => o.PatronId == userId)
                .OrderByDescending(o => o.DateNeeded) // Use proper date field
                .ToList();

            ViewBag.ConsultationHistory = _context.BookALibrarians
                .Where(c => c.PatronId == userId)
                .OrderByDescending(c => c.BookingDate) // corrected from .Time
                .ToList();

            return View();
        }

        public IActionResult ManagePatrons(string searchTerm)
        {
            var query = _context.Patrons
                .Include(p => p.Department)
                .Include(p => p.Program)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.FirstName.Contains(searchTerm) ||
                                         p.LastName.Contains(searchTerm) ||
                                         p.PatronId == searchTerm);
            }
            return View(query.ToList());
        }

        [HttpPost]
        public IActionResult AddPatron([FromBody] Patron p)
        {
            if (p == null) return Json(new { success = false, message = "No data provided." });

            // Validate combination using nullable IDs safely
            if (!IsValidDeptProgById(p.DeptId, p.ProgramId))
                return Json(new { success = false, message = "Invalid Department and Program combination." });

            _context.Patrons.Add(p);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdatePatron([FromBody] Patron updatedPatron)
        {
            if (updatedPatron == null) return Json(new { success = false, message = "No data provided." });

            var patron = _context.Patrons.Find(updatedPatron.PatronId);
            if (patron == null) return Json(new { success = false, message = "Patron not found" });

            if (!IsValidDeptProgById(updatedPatron.DeptId, updatedPatron.ProgramId))
                return Json(new { success = false, message = "Invalid Department and Program combination." });

            patron.FirstName = updatedPatron.FirstName;
            patron.LastName = updatedPatron.LastName;
            patron.MiddleName = updatedPatron.MiddleName;
            patron.DeptId = updatedPatron.DeptId;
            patron.ProgramId = updatedPatron.ProgramId;
            patron.Email = updatedPatron.Email;
            patron.YearLevel = updatedPatron.YearLevel;
            patron.PatronType = updatedPatron.PatronType;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult PatronDashboard()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var patron = _context.Patrons.FirstOrDefault(p => p.PatronId == userIdString);

            ViewBag.PatronName = patron?.FirstName ?? "Dear Patron";

            // Stats for the dashboard
            ViewBag.MyLoansCount = _context.BookBorrowings.Count(b => b.PatronId == userIdString && b.Status != "Returned");
            ViewBag.MyPendingODDS = _context.Odds.Count(o => o.PatronId == userIdString && o.RequestStatus == "Pending");

            return View();
        }

        // HELPER: Accept nullable int? to fix CS1503
        private bool IsValidDeptProgById(int? deptId, int? progId)
        {
            if (!deptId.HasValue || !progId.HasValue) return false;
            return _context.Programs.Any(p => p.ProgramId == progId.Value && p.DeptId == deptId.Value);
        }
    }
}
