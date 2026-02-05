using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        // --- DASHBOARD: RIZAL BUILDING ---
        public IActionResult ReportDashboard()
        {
            var bookings = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Include(s => s.Patron).ThenInclude(p => p.Department)
                .AsNoTracking();

            ViewBag.RBookingCount = bookings
                .Count(s => s.LibrarySeat != null && s.LibrarySeat.Building == "Rizal Building");

            ViewBag.RBookingCountForCollege = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department != null &&
                             sb.Patron.Department.DeptName != "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building");

            ViewBag.RBookingTopDepartment = bookings
                .Where(sb => sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building" &&
                             sb.Patron != null && sb.Patron.Department != null)
                .GroupBy(sb => sb.Patron.Department.DeptName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.Term = "AY 2024-2025 - T1";
            return View();
        }

        // --- REPORT: EINSTEIN BUILDING ---
        public IActionResult BookASeatEinstienReport()
        {
            var bookings = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Include(s => s.Patron).ThenInclude(p => p.Department)
                .AsNoTracking();

            ViewBag.EBookingCount = bookings
                .Count(s => s.LibrarySeat != null && s.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingCountForCollege = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department != null &&
                             sb.Patron.Department.DeptName != "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingCountForSHS = bookings
                .Count(sb => sb.Patron != null && sb.Patron.Department != null &&
                             sb.Patron.Department.DeptName == "SHS" &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            ViewBag.EBookingTopDepartment = bookings
                .Where(sb => sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building" &&
                             sb.Patron != null && sb.Patron.Department != null)
                .GroupBy(sb => sb.Patron.Department.DeptName)
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

        // --- REPORT: BOOK BORROWING ---
        public IActionResult BookBorrowingReport()
        {
            var borrowings = _context.BookBorrowings
                .Include(b => b.Patron).ThenInclude(p => p.Department)
                .Include(b => b.Patron).ThenInclude(p => p.Program)
                .Include(b => b.Book)
                .AsNoTracking();

            ViewBag.BookBorrowCount = borrowings.Count();
            ViewBag.BookBorrowCountForCollege = borrowings.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName != "SHS");
            ViewBag.BookBorrowCountForSHS = borrowings.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName == "SHS");

            ViewBag.BookTopProgram = borrowings
                .Where(bb => bb.Patron != null && bb.Patron.Program != null)
                .GroupBy(bb => bb.Patron.Program.ProgramName)
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

        // --- REPORT: BOOK A LIBRARIAN ---
        public IActionResult BookALibrarianReport()
        {
            var bookings = _context.BookALibrarians
                .Include(b => b.Patron).ThenInclude(p => p.Department)
                .AsNoTracking()
                .ToList();

            ViewBag.LBookingCount = bookings.Count;
            ViewBag.LBookingCountForCollege = bookings.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName != "SHS");
            ViewBag.LBookingCountForSHS = bookings.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName == "SHS");

            ViewBag.LBookingTopDepartment = bookings
                .Where(bb => bb.Patron != null && bb.Patron.Department != null)
                .GroupBy(bb => bb.Patron.Department.DeptName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            return View(bookings);
        }

        // --- REPORT: ODDS ---
        public IActionResult ODDSReports()
        {
            var requests = _context.Odds
                .Include(r => r.Patron).ThenInclude(p => p.Department)
                .AsNoTracking()
                .ToList();

            ViewBag.ODDSCount = requests.Count;
            ViewBag.ODDSCountForCollege = requests.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName != "SHS");
            ViewBag.ODDSCountForSHS = requests.Count(sb => sb.Patron != null && sb.Patron.Department != null && sb.Patron.Department.DeptName == "SHS");

            return View();
        }

        // --- REPORT: GRAMMARLY & TURNITIN ---
        public IActionResult GrammarlyAndTurnitinReport(string service)
        {
            var currentService = string.IsNullOrEmpty(service) ? "Grammarly" : service;
            var requests = _context.Services
                .Include(r => r.Patron).ThenInclude(p => p.Department)
                .Where(gat => gat.ServiceType == currentService)
                .AsNoTracking();

            ViewBag.CurrentService = currentService;
            ViewBag.GATCount = requests.Count();
            ViewBag.GATCountForCollege = requests.Count(gat => gat.Patron != null && gat.Patron.Department != null && gat.Patron.Department.DeptName != "SHS");
            ViewBag.GATCountForSHS = requests.Count(gat => gat.Patron != null && gat.Patron.Department != null && gat.Patron.Department.DeptName == "SHS");

            return View();
        }
    }
}