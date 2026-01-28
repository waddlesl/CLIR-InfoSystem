using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly LibraryDbContext _context;

        public ReportController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult ReportDashboard()
        {
            // Use .Include to join LibrarySeat and Patron tables
            var bookings = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Include(s => s.Patron);

            ViewBag.RBookingCount = bookings
                .Count(s => s.LibrarySeat != null && s.LibrarySeat.Building == "Rizal Building");

            ViewBag.RBookingCountForCollege = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department != "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building");

            ViewBag.RBookingTopDepartment = bookings
                .Where(sb => sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building" && sb.Patron != null)
                .GroupBy(sb => sb.Patron.Department)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.Term = "AY 2024-2025 - T1";
            return View();
        }

        public IActionResult BookASeatEinstienReport()
        {
            var bookings = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Include(s => s.Patron);

            ViewBag.EBookingCount = bookings
                .Count(s => s.LibrarySeat != null && s.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingCountForCollege = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department != "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingCountForSHS = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department == "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingTopDepartment = bookings
                .Where(sb => sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building" && sb.Patron != null)
                .GroupBy(sb => sb.Patron.Department)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.EBookingPreferedSeat = bookings
                .Where(sb => sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building")
                .GroupBy(sb => sb.LibrarySeat.SeatType)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.Term = "AY 2024-2025 - T1";
            return View();
        }

        public IActionResult BookBorrowingReport()
        {
            var borrowings = _context.BookBorrowings
                .Include(b => b.Patron)
                .Include(b => b.Book);

            ViewBag.BookBorrowCount = borrowings.Count();
            ViewBag.BookBorrowCountForCollege = borrowings.Count(sb => sb.Patron != null && sb.Patron.Department != "SHS");
            ViewBag.BookBorrowCountForSHS = borrowings.Count(sb => sb.Patron != null && sb.Patron.Department == "SHS");

            ViewBag.BookTopProgram = borrowings
                .Where(bb => bb.Patron != null)
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var currentYear = DateTime.Now.Year;
            ViewBag.BookTopBooks = borrowings
                .Where(bb => bb.Book != null && bb.BorrowDate.Year == currentYear)
                .GroupBy(bb => bb.Book.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .ToList();

            return View();
        }

        public IActionResult BookALibrarianReport()
        {
            // 1. Rename 'LibrarianBookings' to 'BookALibrarians'
            var bookings = _context.BookALibrarians.Include(b => b.Patron).ToList();

            // 2. Calculate Stats
            ViewBag.LBookingCount = bookings.Count;
            ViewBag.LBookingCountForCollege = bookings.Count(sb => sb.Patron != null && sb.Patron.Department != "SHS");
            ViewBag.LBookingCountForSHS = bookings.Count(sb => sb.Patron != null && sb.Patron.Department == "SHS");

            ViewBag.LBookingTopDepartment = bookings
                .Where(bb => bb.Patron != null)
                .GroupBy(bb => bb.Patron.Department)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            // 3. Pass the bookings list to the View so the table works
            return View(bookings);
        }

        public IActionResult ODDSReports()
        {
            // Fetches from the 'odds' table using the OddsRequest model
            var requests = _context.Odds.Include(r => r.Patron).ToList();

            ViewBag.ODDSCount = requests.Count;
            ViewBag.ODDSCountForCollege = requests.Count(sb => sb.Patron != null && sb.Patron.Department != "SHS");
            ViewBag.ODDSCountForSHS = requests.Count(sb => sb.Patron != null && sb.Patron.Department == "SHS");

            return View();
        }

        public IActionResult GrammarlyAndTurnitinReport(string service)
        {
            var requests = _context.Services.Include(r => r.Patron);

            ViewBag.CurrentService = string.IsNullOrEmpty(service) ? "Grammarly" : service;
            ViewBag.GATCount = requests.Count(gat => gat.ServiceType == service);
            ViewBag.GATCountForCollege = requests.Count(gat => gat.Patron != null && gat.Patron.Department != "SHS" && gat.ServiceType == service);
            ViewBag.GATCountForSHS = requests.Count(gat => gat.Patron != null && gat.Patron.Department == "SHS" && gat.ServiceType == service);

            return View();
        }
    }
}