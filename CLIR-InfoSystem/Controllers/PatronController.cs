using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    // Inherit from BaseController to use shared _context and LogAction
    public class PatronController : BaseController
    {
        public PatronController(LibraryDbContext context) : base(context) { }

        public IActionResult PatronActivity()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            ViewBag.SeatHistory = _context.SeatBookings
                .Include(s => s.TimeSlot)
                .Include(s => s.LibrarySeat)
                .Where(s => s.PatronId == userId)
                .OrderByDescending(s => s.BookingDate)
                .ToList();

            ViewBag.OddsHistory = _context.Odds
                .Where(o => o.PatronId == userId)
                .OrderByDescending(o => o.DateNeeded)
                .ToList();

            ViewBag.ConsultationHistory = _context.BookALibrarians
                .Where(c => c.PatronId == userId)
                .OrderByDescending(c => c.BookingDate)
                .ToList();

            return View("~/Views/Patron/PatronActivity.cshtml");
        }

        [HttpGet]
        public IActionResult GetPatronDetails(string id)
        {
            var p = _context.Patrons.Find(id);
            if (p == null) return NotFound();

            return Json(new
            {
                patronId = p.PatronId,
                firstName = p.FirstName,
                lastName = p.LastName,
                email = p.Email,
                deptId = p.DeptId,
                programId = p.ProgramId
            });
        }

        public IActionResult ManagePatrons(string searchTerm)
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";
            var query = _context.Patrons
                .Include(p => p.Department)
                .Include(p => p.Program)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Safety null-checks for strings during search
                query = query.Where(p => (p.FirstName != null && p.FirstName.Contains(searchTerm)) ||
                                         (p.LastName != null && p.LastName.Contains(searchTerm)) ||
                                         p.PatronId == searchTerm);
            }
            return View("~/Views/Staff/StaffManagePatrons.cshtml",query.ToList());
        }

        [HttpPost]
        public IActionResult AddPatron([FromBody] Patron p)
        {
            if (p == null) return Json(new { success = false, message = "No data provided." });

            if (!IsValidDeptProgById(p.DeptId, p.ProgramId))
                return Json(new { success = false, message = "Invalid Department and Program combination." });

            _context.Patrons.Add(p);

            LogAction($"Registered new patron: {p.FirstName} {p.LastName} ({p.PatronId})", "patrons");
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
            patron.Email = updatedPatron.Email;
            patron.DeptId = updatedPatron.DeptId;
            patron.ProgramId = updatedPatron.ProgramId;

            LogAction($"Updated profile for patron: {patron.PatronId}", "patrons");
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult PatronDashboard()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var patron = _context.Patrons.FirstOrDefault(p => p.PatronId == userIdString);

            ViewBag.PatronName = patron?.FirstName ?? "Dear Patron";
            ViewBag.MyLoansCount = _context.BookBorrowings.Count(b => b.PatronId == userIdString && b.Status != "Returned");
            ViewBag.MyPendingODDS = _context.Odds.Count(o => o.PatronId == userIdString && o.RequestStatus == "Pending");

            return View("~/Views/Patron/PatronDashboard.cshtml");
        }

        private bool IsValidDeptProgById(int? deptId, int? progId)
        {
            if (deptId == null) return true;
            if (progId == null) return false;
            return _context.Programs.Any(p => p.ProgramId == progId.Value && p.DeptId == deptId.Value);
        }
    }
}