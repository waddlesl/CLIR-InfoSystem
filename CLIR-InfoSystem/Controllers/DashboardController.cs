using Microsoft.AspNetCore.Mvc;
using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CLIR_InfoSystem.Controllers
{
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
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // 1. Pending Count (Corrected to check all tables)
            int pendingOdds = _context.Odds.Count(o => o.PatronId == userId && o.RequestStatus == "Pending");
            int pendingServices = _context.Services.Count(s => s.PatronId == userId && s.RequestStatus == "Pending");
            int pendingLibrarian = _context.BookALibrarians.Count(b => b.PatronId == userId && b.Status == "Pending");
            int pendingBorrowing = _context.BookBorrowings.Count(b => b.PatronId == userId && b.Status == "Reserved");

            ViewBag.MyPendingRequests = pendingOdds + pendingServices + pendingLibrarian + pendingBorrowing;

            // 2. Simple Stats
            ViewBag.MyLoans = _context.BookBorrowings.Count(b => b.PatronId == userId && b.Status == "Borrowed");
            ViewBag.ActiveSeats = _context.SeatBookings.Count(b => b.PatronId == userId && b.Status == "Reserved");

            // 3. Activity Feed (Using corrected property names)
            var odds = _context.Odds.Where(o => o.PatronId == userId)
                .Select(o => new {
                    Type = "ODDS",
                    Desc = o.MaterialType, // Changed from TypeOfMaterial to MaterialType
                    Status = o.RequestStatus,
                    Date = o.RequestDate
                }).ToList();

            var services = _context.Services.Where(s => s.PatronId == userId)
                .Select(s => new {
                    Type = "Service",
                    Desc = s.ServiceType,
                    Status = s.RequestStatus,
                    Date = (DateTime)s.RequestDate // Ensure cast to DateTime
                }).ToList();

            var consults = _context.BookALibrarians.Where(b => b.PatronId == userId)
                .Select(b => new {
                    Type = "Consultation",
                    Desc = b.Topic,
                    Status = b.Status,
                    Date = b.BookingDate
                }).ToList();

            // Combine and finalize
            ViewBag.AllActivities = odds.Select(x => new { ServiceType = x.Type, Description = x.Desc, Status = x.Status, Date = x.Date, StatusClass = GetStatusClass(x.Status) })
                .Concat(services.Select(x => new { ServiceType = x.Type, Description = x.Desc, Status = x.Status, Date = x.Date, StatusClass = GetStatusClass(x.Status) }))
                .Concat(consults.Select(x => new { ServiceType = x.Type, Description = x.Desc, Status = x.Status, Date = (DateTime)x.Date, StatusClass = GetStatusClass(x.Status) }))
                .OrderByDescending(a => a.Date)
                .ToList();

            return View();
        }

        // REMEMBER: Make this static as requested by the previous error
        private static string GetStatusClass(string status) => status switch
        {
            "Pending" => "bg-status-pending",
            "Approved" or "Reserved" => "bg-status-approved",
            "Completed" or "Fulfilled" => "bg-status-completed",
            "Cancelled" => "bg-status-cancelled",
            "Rejected" or "Expired" => "bg-status-rejected",
            _ => "bg-light"
        };

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
            ViewBag.OverdueCount = _context.BookBorrowings.Count(b => b.DueDate < DateTime.Now && b.Status == "Borrowed");
            ViewBag.PendingServices = _context.Services.Count(s => s.RequestStatus == "Pending");
            ViewBag.PendingConsultations = _context.BookALibrarians.Count(b => b.Status == "Pending");
            ViewBag.PendingOdds = _context.Odds.Count(o => o.RequestStatus == "Pending");
            ViewBag.DigitalQueue = ViewBag.PendingServices + ViewBag.PendingOdds;
            ViewBag.TodaySeats = _context.SeatBookings.Count(s => s.BookingDate == DateTime.Today && s.Status == "Reserved");

            ViewBag.RecentTransactions = _context.BookBorrowings
                .OrderByDescending(b => b.BorrowDate)
                .Take(5)
                .ToList();

            LogAction($"Viewed {staffType} Dashboard", "dashboard");
            _context.SaveChanges();
            return View($"{staffType.Replace(" ", "")}Dashboard");
        }

       
    }
}