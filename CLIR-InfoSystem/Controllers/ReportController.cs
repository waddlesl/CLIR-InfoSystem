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
            .GroupBy(bb => bb.Patron.Department)
            .OrderByDescending(g => g.Count(sb => sb.Building == "Rizal"))
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
                .GroupBy(bb => bb.Patron.Department)
                .OrderByDescending(g => g.Count(sb => sb.Building == "Einstein"))
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
            ViewBag.LBookingCount = _context.SeatBookings.Count();
            ViewBag.LBookingCountForCollege = _context.SeatBookings.Count(sb => sb.Patron.Department != "SHS");
            ViewBag.LBookingCountForSHS = _context.SeatBookings.Count(sb => sb.Patron.Department == "SHS");
            ViewBag.LBookingTopDepartment = _context.SeatBookings
                .GroupBy(bb => bb.Patron.Department)
                .OrderByDescending(g => g.Count(sb => sb.Building == "Einstien"))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            ViewBag.LBookProgram = _context.BookBorrowings
                .GroupBy(bb => bb.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToList();


            return View();
        }

        public IActionResult ODDSReports(string service = "Grammarly")
        {
            // Store the current service to highlight the active tab in the UI
            ViewBag.CurrentService = service;

            
            var filteredData = _context.ServiceRequests
                .Where(sr => sr.ServiceType == service);

            ViewBag.TotalReservations = filteredData.Count();

            // Most Dept. Utilized for the SPECIFIC service selected
            ViewBag.MostUtilizedDept = filteredData
                .GroupBy(sr => sr.Patron.Program)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            // You can use a switch statement if logic differs greatly between services
            switch (service)
            {
                case "Grammarly":
                    ViewBag.SectionTitle = "Grammarly Usage Statistics";
                    // Add Grammarly-specific logic here
                    break;
                case "Turnitin":
                    ViewBag.SectionTitle = "Turnitin Submission Reports";
                    break;
                default:
                    ViewBag.SectionTitle = service;
                    break;
            }

            return View();
        }


    }
}
