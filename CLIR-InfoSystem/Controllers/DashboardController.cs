using Microsoft.AspNetCore.Mvc;
using CLIR_InfoSystem.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CLIR_InfoSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly LibraryDbContext _context;

        public DashboardController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login", "Account");

            return role switch
            {
                "Admin" => RedirectToAction("AdminDashboard"),
                "Librarian" => RedirectToAction("LibrarianDashboard"), // Matches AccountController role
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
                // Removed int.TryParse because PatronId is a string in your Model
                ViewBag.MyLoans = _context.BookBorrowings
                    .Count(b => b.PatronId == userId && b.Status != "Returned");

                ViewBag.MyPendingRequests = _context.Services
                    .Count(s => s.PatronId == userId && s.RequestStatus == "Pending");
            }
            else
            {
                ViewBag.MyLoans = 0;
            }

            return View();
        }

        public IActionResult AdminDashboard()
        {
            ViewBag.StaffCount = _context.Staff.Count();
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.PatronCount = _context.Patrons.Count();
            ViewBag.ActiveLoans = _context.BookBorrowings.Count(b => b.Status == "Borrowed");

            return View();
        }

        public IActionResult LibrarianDashboard() => StaffCommonView();
        public IActionResult StudentAssistantDashboard() => StaffCommonView();

        private IActionResult StaffCommonView()
        {
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.PatronCount = _context.Patrons.Count();
            ViewBag.BorrowedCount = _context.BookBorrowings.Count(b => b.Status == "Borrowed");

            return View("StaffCommonDashboard");
        }
    }
}