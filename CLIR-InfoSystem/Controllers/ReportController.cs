using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;

namespace CLIR_InfoSystem.Controllers
{
    public class ReportController: Controller
    {
        private readonly LibraryDbContext _context;

        public ReportController(LibraryDbContext context)
        {
            _context = context;
        }
        public IActionResult ReportDashboard()
        {
            ViewBag.RBookingCount = _context.SeatBookings.Count(s => s.Building == "Rizal");
            ViewBag.RBookingCountForCollege = _context.SeatBookings.Count(sb => sb.Patron.Department != "SHS" && sb.Building == "Rizal");
            ViewBag.RBookingCountForSHS = _context.SeatBookings.Count(sb => sb.Patron.Department == "SHS" && sb.Building == "Rizal");
            ViewBag.RBookingTopDepartment = _context.SeatBookings
            .Where(sb => sb.Patron.Department == "Rizal")
            .GroupBy(sb => sb.Patron.Department)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "N/A";

            ViewBag.RBookingPreferedSeat = _context.SeatBookings
                .Where(sb => sb.Building == "Rizal")
                .GroupBy(sb => sb.PreferredSeating)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.Term = "AY 2024-2025 - T1";
            return View();
        }

        public IActionResult BookASeatEinstienReport()
        {
            ViewBag.EBookingCount = _context.SeatBookings.Count(s => s.Building == "Einstein");
            ViewBag.EBookingCountForCollege = _context.SeatBookings.Count(sb => sb.Patron.Department != "SHS" && sb.Building == "Einstein");
            ViewBag.EBookingCountForSHS = _context.SeatBookings.Count(sb => sb.Patron.Department == "SHS" && sb.Building == "Einstein");
            ViewBag.EBookingTopDepartment = _context.SeatBookings
                .Where(sb => sb.Patron.Department == "Einstein")
                .GroupBy(sb => sb.PreferredSeating)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.EBookingPreferedSeat = _context.SeatBookings
                .Where(sb => sb.Building == "Einstein")
                .GroupBy(bb => bb.PreferredSeating)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.Term = "AY 2024-2025 - T1";
            return View();
        }

        public IActionResult BookBorrowingReport()
        {
            ViewBag.BookBorrowCount = _context.BookBorrowings.Count();
            ViewBag.BookBorrowCountForCollege = _context.BookBorrowings.Count(sb => sb.Patron.Department != "SHS");
            ViewBag.BookBorrowCountForSHS = _context.BookBorrowings.Count(sb => sb.Patron.Department == "SHS");
            ViewBag.BookTopProgram = _context.BookBorrowings
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var currentYear = DateTime.Now.Year; // to get the year change later if we want it to be dynamic
            ViewBag.BookTopBooks = _context.BookBorrowings
                .Where(bb => bb.BorrowDate.Year == currentYear)
                .GroupBy(bb => bb.Book.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .ToList();

            
            return View();
        }

        public IActionResult BookALibrarianReport()
        {
            ViewBag.LBookingCount = _context.LibrarianBookings.Count();
            ViewBag.LBookingCountForCollege = _context.LibrarianBookings.Count(sb => sb.Patron.Department != "SHS");
            ViewBag.LBookingCountForSHS = _context.LibrarianBookings.Count(sb => sb.Patron.Department == "SHS");
            ViewBag.LBookingTopDepartment = _context.LibrarianBookings
                .GroupBy(bb => bb.Patron.Department)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.LBookProgram = _context.LibrarianBookings
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToList();


            return View();
        }

        public IActionResult ODDSReports()
        {
            ViewBag.ODDSCount = _context.ServiceRequests.Count();
            ViewBag.ODDSCountForCollege = _context.ServiceRequests.Count(sb => sb.Patron.Department != "SHS");
            ViewBag.ODDSCountForSHS = _context.ServiceRequests.Count(sb => sb.Patron.Department == "SHS");

            ViewBag.ODDSProgram = _context.ServiceRequests
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToList();


            return View();
        }
        public IActionResult GrammarlyAndTurnitinReport(string service)
        {
            ViewBag.CurrentService = string.IsNullOrEmpty(service) ? "Grammarly" : service;
            ViewBag.GATCount = _context.GrammarlyAndTurnitinRequests.Count(gat => gat.ServiceType == service);
            ViewBag.GATCountForCollege = _context.GrammarlyAndTurnitinRequests.Count(gat => gat.Patron.Department != "SHS" && gat.ServiceType == service);
            ViewBag.GATCountForSHS = _context.GrammarlyAndTurnitinRequests.Count(gat => gat.Patron.Department == "SHS" && gat.ServiceType == service);

            ViewBag.GATProgram = _context.GrammarlyAndTurnitinRequests
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.CurrentService = service;
            return View();
        }

    }
}
