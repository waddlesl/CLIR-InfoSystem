using Azure.Core;
using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CLIR_InfoSystem.Controllers
{
    // Inheriting from BaseController to use shared context
    public class ReportController : BaseController
    {
        public ReportController(LibraryDbContext context) : base(context)
        {
        }

        //repeatable filter
        private (DateTime Start, DateTime End, int Year) GetTermDates(int selectedYear, int selectedTerm)
        {
            // Default to current Academic Year if none selected
            if (selectedYear == 0)
            {
                selectedYear = (DateTime.Now.Month >= 8) ? DateTime.Now.Year : DateTime.Now.Year - 1;
            }

            DateTime startDate;
            DateTime endDate;

            switch (selectedTerm)
            {
                case 1:
                    startDate = new DateTime(selectedYear, 8, 1);
                    endDate = new DateTime(selectedYear, 11, 30);
                    break;
                case 2:
                    startDate = new DateTime(selectedYear, 12, 1);
                    endDate = new DateTime(selectedYear + 1, 3, 31);
                    break;
                case 3:
                    startDate = new DateTime(selectedYear + 1, 5, 1);
                    endDate = new DateTime(selectedYear + 1, 7, 31);
                    break;
                default: //Full year
                    startDate = new DateTime(selectedYear, 8, 1);
                    endDate = new DateTime(selectedYear + 1, 7, 31);
                    break;
            }

            return (startDate, endDate, selectedYear);
        }




        public IActionResult ReportDashboard(int selectedYear, int selectedTerm)
        {
            var dateRange = GetTermDates(selectedYear, selectedTerm);

            var dailyCounts = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Where(s => s.BookingDate >= dateRange.Start && s.BookingDate <= dateRange.End &&
                            (int)s.BookingDate.DayOfWeek >= 1 && (int)s.BookingDate.DayOfWeek <= 6 &&
                            s.LibrarySeat.Building == "Rizal Building")
                .GroupBy(s => new { s.BookingDate.Date, s.BookingDate.DayOfWeek })
                .Select(g => new {
                    DayName = g.Key.DayOfWeek,
                    Count = g.Count()
                })
                .ToList();

            var averageVisitors = dailyCounts
                .GroupBy(g => g.DayName)
                .Select(g => new {
                    Day = g.Key.ToString(),
                    Average = Math.Round(g.Average(x => x.Count), 0), // 0 for whole numbers
                    Order = (int)g.Key
                })
                .OrderBy(g => g.Order)
                .ToList();

            ViewBag.VisitorLabels = JsonSerializer.Serialize(averageVisitors.Select(v => v.Day));
            ViewBag.VisitorData = JsonSerializer.Serialize(averageVisitors.Select(v => v.Average));

            var bookings = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Include(s => s.Patron).ThenInclude(p => p.Department)
                .Where(s => s.BookingDate >= dateRange.Start && s.BookingDate <= dateRange.End);
            ViewBag.RBookingCount = bookings.Count(sb => sb.LibrarySeat.Building == "Rizal Building" && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End);
            ViewBag.RBookingCountForCollege = bookings.Count(sb =>sb.Patron.DeptId != 9 && sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building" && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End);
            ViewBag.RBookingCountForSHS = bookings.Count(sb => sb.Patron.DeptId == 9 && sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building" && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End);


            var topDept = bookings
                .Where(sb => sb.LibrarySeat.Building == "Rizal Building" && sb.Patron != null && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End)
                .GroupBy(sb => sb.Patron.Department.DeptCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            ViewBag.RBookingTopDepartment = topDept ?? "N/A";
            ViewBag.Term = $"AY {dateRange.Year}-{dateRange.Year + 1} - T{selectedTerm}";

            return View();
        }
  

        public IActionResult BookASeatEinstienReport(int selectedYear, int selectedTerm)
        {
            var dateRange = GetTermDates(selectedYear, selectedTerm);

            var dailyCounts = _context.SeatBookings
                .Include(s => s.LibrarySeat)
                .Where(s => s.BookingDate >= dateRange.Start && s.BookingDate <= dateRange.End &&
                            (int)s.BookingDate.DayOfWeek >= 1 && (int)s.BookingDate.DayOfWeek <= 6 &&
                            s.LibrarySeat.Building == "Einstein Building")
                .GroupBy(s => new { s.BookingDate.Date, s.BookingDate.DayOfWeek })
                .Select(g => new {
                    DayName = g.Key.DayOfWeek,
                    Count = g.Count()
                })
                .ToList();

            var averageVisitors = dailyCounts
                .GroupBy(g => g.DayName)
                .Select(g => new {
                    Day = g.Key.ToString(),
                    Average = Math.Round(g.Average(x => x.Count), 0), // 0 for whole numbers
                    Order = (int)g.Key
                })
                .OrderBy(g => g.Order)
                .ToList();

            ViewBag.VisitorLabels = JsonSerializer.Serialize(averageVisitors.Select(v => v.Day));
            ViewBag.VisitorData = JsonSerializer.Serialize(averageVisitors.Select(v => v.Average));


            var bookings = _context.SeatBookings
               .Include(s => s.LibrarySeat)
               .Include(s => s.Patron).ThenInclude(p => p.Department)
               .Where(s => s.BookingDate >= dateRange.Start && s.BookingDate <= dateRange.End);
            ViewBag.EBookingCount = bookings.Count(sb => sb.LibrarySeat.Building == "Einstein Building");
            ViewBag.EBookingCountForCollege = bookings.Count(sb => sb.Patron.DeptId != 9 && sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building" && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End);
            ViewBag.EBookingCountForSHS = bookings.Count(sb => sb.Patron.DeptId == 9 && sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building" && sb.BookingDate >= dateRange.Start && sb.BookingDate <= dateRange.End);


            var topDept = bookings
                .Where(sb => sb.LibrarySeat.Building == "Einstein Building" && sb.Patron != null)
                .GroupBy(sb => sb.Patron.Department.DeptCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            ViewBag.EBookingTopDepartment = topDept ?? "N/A";
            ViewBag.Term = $"AY {dateRange.Year}-{dateRange.Year + 1} - T{selectedTerm}";
            return View();
        }

        public IActionResult BookBorrowingReport(int selectedYear, int selectedTerm)
        {
            var dateRange = GetTermDates(selectedYear, selectedTerm);
            var borrowings = _context.BookBorrowings
                .Include(b => b.Patron)
                .ThenInclude(p => p.Program)
                .Include(b => b.Book)
                .Where(s => s.ReturnDate >= dateRange.Start && s.ReturnDate <= dateRange.End);

            ViewBag.BookBorrowCount = borrowings.Count();
            ViewBag.BookBorrowCountForCollege = borrowings.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            ViewBag.BookBorrowCountForSHS = borrowings.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);

            ViewBag.BookTopProgram = borrowings
                .Where(bb => bb.Patron != null)
                .GroupBy(bb => bb.Patron.Program.ProgramCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var currentYear = DateTime.Now.Year;
            ViewBag.BookTopBooks = borrowings
                .Where(bb => bb.Book != null)
                .GroupBy(bb => bb.Book.Title)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .ToList();

            return View();
        }

        public IActionResult BookALibrarianReport(int selectedYear, int selectedTerm)
        {
            var dateRange = GetTermDates(selectedYear, selectedTerm);
            var bookings = _context.BookALibrarians.Include(b => b.Patron).ThenInclude(p => p.Program).ThenInclude(p => p.Department).Where(s => s.BookingDate >= dateRange.Start && s.BookingDate <= dateRange.End).ToList();

            ViewBag.LBookingCount = bookings.Count;
            ViewBag.LBookingCountForCollege = bookings.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            ViewBag.LBookingCountForSHS = bookings.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);

            ViewBag.LBookingTopDepartment = bookings
                .Where(bb => bb.Patron != null)
                .GroupBy(bb => bb.Patron.Department.DeptCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var currentYear = DateTime.Now.Year;
            ViewBag.LBookingTopProgram = bookings
                .Where(bb => bb.BookingDate.Year == currentYear)
                .GroupBy(bb => bb.Patron.Program.ProgramCode)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToList();


            return View(bookings);
        }

        public IActionResult ODDSReports(int selectedYear, int selectedTerm)
        {
            var dateRange = GetTermDates(selectedYear, selectedTerm);
            var requests = _context.Odds.Include(b => b.Patron).ThenInclude(p => p.Program).ThenInclude(p => p.Department).Where(s => s.RequestDate >= dateRange.Start && s.RequestDate <= dateRange.End).ToList();

            ViewBag.ODDSCount = requests.Count;
            ViewBag.ODDSCountForCollege = requests.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            ViewBag.ODDSCountForSHS = requests.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);

            var currentYear = DateTime.Now.Year;
            ViewBag.ODDSProgram = requests
                .Where(bb => bb.RequestDate.Year == currentYear)
                .GroupBy(bb => bb.Patron.Program.ProgramCode)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToList();


            return View();
        }

        public IActionResult GrammarlyAndTurnitinReport(string service, int selectedYear, int selectedTerm)
        {
            string activeService = string.IsNullOrEmpty(service) ? "Grammarly" : service;
            ViewBag.CurrentService = activeService;
            var dateRange = GetTermDates(selectedYear, selectedTerm);
            var requests = _context.Services.Include(b => b.Patron).ThenInclude(p => p.Program).ThenInclude(p => p.Department).Where(s => s.RequestDate >= dateRange.Start && s.RequestDate <= dateRange.End && s.ServiceType == activeService).ToList();


            ViewBag.GATCount = requests.Count();
            ViewBag.GATCountForCollege = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId != 9);
            ViewBag.GATCountForSHS = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId == 9);
            ViewBag.GATMostDept = requests
                .Where(bb => bb.Patron != null)
                .GroupBy(bb => bb.Patron.Department.DeptCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            return View();
        }

        [HttpGet("download-stats")]
        public async Task<IActionResult> DownloadReport(int selectedYear, int? selectedTerm)
        {
            try
            {
                DateTime startDate;
                DateTime endDate;
                if (selectedTerm == 1)
                {
                    startDate = new DateTime(selectedYear, 8, 1);
                    endDate = new DateTime(selectedYear, 11, 30);
                }
                else if (selectedTerm == 2)
                {
                    startDate = new DateTime(selectedYear, 12, 1);
                    endDate = new DateTime(selectedYear + 1, 3, 31);
                }
                else if (selectedTerm == 3)
                {
                    startDate = new DateTime(selectedYear + 1, 5, 1);
                    endDate = new DateTime(selectedYear + 1, 7, 31);
                }
                else
                {
                    startDate = new DateTime(selectedYear, 8, 1);
                    endDate = new DateTime(selectedYear + 1, 7, 31);
                }

                var pdfBuffer = await ExportPdfAsync(startDate, endDate, selectedYear, selectedTerm ?? 0);

                return File(pdfBuffer, "application/pdf", $"UsageReport.pdf_{selectedYear}_Term{selectedTerm}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating PDF: {ex.Message}");
            }
        }

        private async Task<byte[]> ExportPdfAsync(DateTime startDate, DateTime endDate, int yearReport, int? termReport)
        {
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();

            string html = GetHtmlContent(startDate, endDate, yearReport, termReport);

            await page.SetContentAsync(html);

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "20px", Bottom = "20px", Left = "20px", Right = "20px" }
            });

            await browser.CloseAsync();
            return pdfBytes;
        }

        private string GetHtmlContent(DateTime startDate, DateTime endDate, int yearReport, int? termReport)
        {
            var patrons = _context.Patrons
                .Include(p => p.Program)
                .ToList();

            var PatronTotal = patrons.Count();
            var PatronSHS = patrons.Count(p => p.DeptId == 9);
            var PatronCollege = patrons.Count(p => p.DeptId != 9);

            var PatronTopProgram = patrons
                .Where(p => p.Program != null)
                .GroupBy(p => p.Program.ProgramCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var requests = _context.Services.Include(r => r.Patron).Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate);

            var GrammarlyCountForCollege = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId != 9 && gat.ServiceType == "Grammarly");
            var GrammarlyCountForSHS = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId == 9 && gat.ServiceType == "Grammarly");
            var TurnitinCountForCollege = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId != 9 && gat.ServiceType == "Turnitin");
            var TurnitinCountForSHS = requests.Count(gat => gat.Patron != null && gat.Patron.DeptId == 9 && gat.ServiceType == "Turnitin");

            var bookings = _context.BookALibrarians.Include(b => b.Patron).Where(x => x.BookingDate >= startDate && x.BookingDate <= endDate).ToList();

            var LBookingCountForCollege = bookings.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            var LBookingCountForSHS = bookings.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);

            var borrowings = _context.BookBorrowings.Include(b => b.Patron).Include(b => b.Book).Where(x => x.BorrowDate >= startDate && x.BorrowDate <= endDate);

            var BookBorrowCountForCollege = borrowings.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            var BookBorrowCountForSHS = borrowings.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);

            var TopProgram = borrowings
                .Where(bb => bb.Patron != null && bb.Patron.Program != null)
                .GroupBy(bb => bb.Patron.Program.ProgramCode)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var BookTopProgram = TopProgram ?? "N/A";

            var currentYear = DateTime.Now.Year;
            var BookTopBooks = borrowings
                .Where(bb => bb.Book != null && bb.BorrowDate.Year == currentYear)
                .GroupBy(bb => bb.Book.Title)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var Sbookings = _context.SeatBookings.Include(s => s.LibrarySeat).Include(s => s.Patron).Where(x => x.BookingDate >= startDate && x.BookingDate <= endDate);

            var RBookingCountForCollege = Sbookings
                .Count(sb => sb.Patron != null && sb.Patron.DeptId != 9 &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building");

            var RBookingCountForSHS = Sbookings
                .Count(sb => sb.Patron != null && sb.Patron.DeptId == 9 &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Rizal Building");

            var EBookingCountForCollege = Sbookings
                .Count(sb => sb.Patron != null && sb.Patron.DeptId != 9 &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            var EBookingCountForSHS = Sbookings
                .Count(sb => sb.Patron != null && sb.Patron.DeptId == 9 &&
                             sb.LibrarySeat != null && sb.LibrarySeat.Building == "Einstein Building");

            var odds = _context.Odds.Include(r => r.Patron).Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate).ToList();

            var ODDSCountForCollege = odds.Count(sb => sb.Patron != null && sb.Patron.DeptId != 9);
            var ODDSCountForSHS = odds.Count(sb => sb.Patron != null && sb.Patron.DeptId == 9);
            var ODDSCountForScanning = odds.Count(sb => sb.Patron != null && sb.ServiceType == "Scanning");
            var ODDSCountForThesis = odds.Count(sb => sb.Patron != null && sb.MaterialType == "Thesis");
            var ODDSCountForResource = odds.Count(sb => sb.Patron != null && sb.ServiceType == "Resource Link");
            var ODDSCountForJournal = odds.Count(sb => sb.Patron != null && sb.MaterialType == "Journal Article");

            return $$""""
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body { font-family: 'Verdana', sans-serif; margin: 0; padding: 20px; color: #1a2035; }
                        .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
                        .header-left h1 { color: #CC0000; margin: 0; font-size: 24px; }
                        .header-left p { margin: 0; font-size: 12px; }
                        .header-right { text-align: right; }
                        .header-right h2 { margin: 0; font-size: 16px; color: #1a2035; }
                        .header-right h1 { margin: 0; font-size: 28px; font-weight: 900; }
                        .main-grid { display: grid; grid-template-columns: repeat(12, 1fr); gap: 10px; }
                        .block { border: 1px solid #e0e0e0; background: white; display: flex; flex-direction: column; min-height: 120px;}
                        .block-header { background: #CC0000; color: white; text-align: center; padding: 5px; font-size: 11px; font-weight: bold;}
                        .span-4 { grid-column: span 4;}
                        .span-8 { grid-column: span 8;}
                        .span-10 { grid-column: span 10;}
                        .span-12 { grid-column: span 12;}
                        .row-span-2 { grid-row: span 2;}
                        .stats-row { display: flex; justify-content: space-around; padding: 10px; flex-grow: 1; align-items: center;}
                        .stat-item { text-align: center; flex: 1;}
                        .stat-value { font-size: 18px; font-weight: 900; margin-bottom: 2px; white-space: nowrap;}
                        .stat-label { background: #CC0000; color: white; font-size: 9px; padding: 2px 5px; font-weight: bold; display: inline-block;}
                        .stat-sub { font-size: 9px; margin-top: 2px;}
                        .divider { width: 1px; background: #000; height: 40px;}
                        .big-value { font-size: 42px; font-weight: 900; margin: 10px 0;}
                    </style>
                </head>
                <body>
                    <div class="header">
                        <div class="header-left">
                            <h1>MAPÚA MCL</h1>
                            <p>CENTER FOR LEARNING AND INFORMATION RESOURCES</p>
                        </div>
                        <div class="header-right">
                            <h2>FIRST TERM A.Y. 2024 - 2025</h2>
                            <h1>USAGE STATISTICS REPORT</h1>
                        </div>
                    </div>
                    <div class="main-grid">
                        <div class="block span-4 row-span-2">
                            <div class="block-header">Patrons</div>
                            <div style="text-align: center; padding: 10px;">
                                <div class="big-value">{{PatronTotal}}</div>
                                <div class="stat-label">Total Number of Patrons</div>
                            </div>
                            <div class="stats-row">
                                <div class="stat-item">
                                    <div class="stat-value">{{PatronCollege}}</div>
                                    <div class="stat-label">COLLEGE</div>
                                </div>
                                <div class="divider"></div>
                                <div class="stat-item">
                                    <div class="stat-value">{{PatronSHS}}</div>
                                    <div class="stat-label">SHS</div>
                                </div>
                            </div>
                            <div style="text-align: center; padding: 10px;">
                                <div class="stat-value">{{PatronTopProgram}}</div>
                                <div class="stat-label">Top Program</div>
                            </div>
                        </div>
                        <div class="block span-8">
                            <div class="block-header">Book-A-Seat Service</div>
                            <div style="text-align: center; font-size: 12px; font-weight: bold; padding-top: 5px;">Total number of reservations</div>
                            <div class="stats-row">
                                <div class="stat-item"><div class="stat-value">{{EBookingCountForCollege}}</div><div class="stat-label">CLIR Einstein</div><div class="stat-sub">(College)</div></div>
                                <div class="stat-item"><div class="stat-value">{{RBookingCountForCollege}}</div><div class="stat-label">CLIR Rizal</div><div class="stat-sub">(College)</div></div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{EBookingCountForSHS}}</div><div class="stat-label">CLIR Einstein</div><div class="stat-sub">(SHS)</div></div>
                                <div class="stat-item"><div class="stat-value">{{RBookingCountForSHS}}</div><div class="stat-label">CLIR Rizal</div><div class="stat-sub">(SHS)</div></div>
                            </div>
                        </div>
                        <div class="block span-8">
                            <div class="block-header">Online Document Delivery Service</div>
                            <div class="stats-row">
                                <div class="stat-item"><div class="stat-value">{{ODDSCountForScanning}}</div><div class="stat-label">COLLEGE</div><div class="stat-sub">Scanning requests</div></div>
                                <div class="stat-item"><div class="stat-value">{{ODDSCountForThesis}}</div><div class="stat-label">COLLEGE</div><div class="stat-sub">Thesis Access</div></div>
                                <div class="stat-item"><div class="stat-value">{{ODDSCountForResource}}</div><div class="stat-label">COLLEGE</div><div class="stat-sub">Resource Link</div></div>
                                <div class="stat-item"><div class="stat-value">{{ODDSCountForJournal}}</div><div class="stat-label">COLLEGE</div><div class="stat-sub">Journal Articles</div></div>
                            </div>
                        </div>
                        <div class="block span-4">
                            <div class="block-header">Grammarly</div>
                            <div style="text-align: center; font-size: 12px; font-weight: bold; padding-top: 5px;">Number of Access Provided</div>
                            <div class="stats-row">
                                <div class="stat-item"><div class="stat-value">{{GrammarlyCountForCollege}}</div><div class="stat-label">COLLEGE</div></div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{GrammarlyCountForSHS}}</div><div class="stat-label">SHS</div></div>
                            </div>
                        </div>
                        <div class="block span-4">
                            <div class="block-header">Turnitin</div>
                            <div style="text-align: center; font-size: 12px; font-weight: bold; padding-top: 5px;">Number of Access Provided</div>
                            <div class="stats-row">
                                <div class="stat-item"><div class="stat-value">{{TurnitinCountForCollege}}</div><div class="stat-label">COLLEGE</div></div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{TurnitinCountForSHS}}</div><div class="stat-label">SHS</div></div>
                            </div>
                        </div>
                        <div class="block span-4">
                            <div class="block-header">Book-A-Librarian</div>
                            <div style="text-align: center; font-size: 12px; font-weight: bold; padding-top: 5px;">Number of Sessions Conducted</div>
                            <div class="stats-row">
                                <div class="stat-item"><div class="stat-value">{{LBookingCountForCollege}}</div><div class="stat-label">COLLEGE</div></div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{LBookingCountForSHS}}</div><div class="stat-label">SHS</div></div>
                            </div>
                        </div>
                        <div class="block span-12">
                            <div class="block-header">Book Borrowing</div>
                            <div style="text-align: center; font-size: 12px; font-weight: bold; padding-top: 5px;">Number of Books Borrowed</div>
                            <div class="stats-row">
                                <div class="stats-row">
                                    <div class="stat-item"><div class="stat-value">{{BookBorrowCountForCollege}}</div><div class="stat-label">COLLEGE</div></div>
                                    <div class="divider"></div>
                                    <div class="stat-item"><div class="stat-value">{{BookBorrowCountForSHS}}</div><div class="stat-label">SHS</div></div>
                                </div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{BookTopProgram}}</div><div class="stat-label">Top Borrower (Program)</div></div>
                                <div class="divider"></div>
                                <div class="stat-item"><div class="stat-value">{{BookTopBooks}}</div><div class="stat-label">Most Borrowed Book</div></div>
                            </div>
                        </div>
                    </div>
                </body>
                </html>
                """";
        }
        //private (DateTime start, DateTime end) GetTermDates(int year, int term)
        //{
        //    int rYear = (year == 0) ? GetDefaultYear() : year;
        //    return term switch
        //    {
        //        1 => (new DateTime(rYear, 8, 1), new DateTime(rYear, 11, 30)),
        //        2 => (new DateTime(rYear, 12, 1), new DateTime(rYear + 1, 3, 31)),
        //        3 => (new DateTime(rYear + 1, 5, 1), new DateTime(rYear + 1, 7, 31)),
        //        _ => (new DateTime(rYear, 8, 1), new DateTime(rYear + 1, 7, 31))
        //    };
        //}

        //private int GetDefaultYear() => (DateTime.Now.Month >= 8) ? DateTime.Now.Year : DateTime.Now.Year - 1;
    }


}