using Microsoft.AspNetCore.Mvc;
using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CLIR_InfoSystem.Controllers
{
    // Inherit from BaseController to use shared _context and LogAction
    public class DashboardController : BaseController
    {
        public DashboardController(LibraryDbContext context) : base(context) { }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login", "Account");

            return role switch
            {
                "Admin" => RedirectToAction("AdminDashboard"),
                "Librarian" => RedirectToAction("LibrarianDashboard"),
                "Student Assistant" => RedirectToAction("StudentAssistantDashboard"),
                "Patron" => RedirectToAction("PatronDashboard"),
                _ => RedirectToAction("Login", "Account")
            };
        }

        public IActionResult PatronDashboard()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userId))
            {
                ViewBag.MyLoans = _context.BookBorrowings
                    .Count(b => b.PatronId == userId && b.Status != "Returned");

                ViewBag.MyPendingRequests = _context.Services
                    .Count(s => s.PatronId == userId && s.RequestStatus == "Pending");

                // Log view access if desired
                LogAction("Viewed Patron Dashboard", "dashboard");
                _context.SaveChanges();
            }
            else
            {
                ViewBag.MyLoans = 0;
            }

            return View();
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return Unauthorized();

            ViewBag.StaffCount = _context.Staff.Count();
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.PatronCount = _context.Patrons.Count();
            ViewBag.ActiveLoans = _context.BookBorrowings.Count(b => b.Status == "Borrowed");

            LogAction("Viewed Admin Dashboard", "dashboard");
            _context.SaveChanges();

            return View();
        }

        public IActionResult LibrarianDashboard() => StaffCommonView("Librarian");
        public IActionResult StudentAssistantDashboard() => StaffCommonView("Student Assistant");

        private IActionResult StaffCommonView(string staffType)
        {
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.PatronCount = _context.Patrons.Count();
            ViewBag.BorrowedCount = _context.BookBorrowings.Count(b => b.Status == "Borrowed");

            LogAction($"Viewed {staffType} Dashboard", "dashboard");
            _context.SaveChanges();

            return View("StaffCommonDashboard");
        }
    }
}