using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CLIR_InfoSystem.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(LibraryDbContext context) : base(context) { }

        // SeedData: Populates the database with initial Admin and Staff accounts if the 'admin' user doesn't exist
        [HttpGet]
        public IActionResult SeedData()
        {
            // Checks if the database is already populated to prevent duplicates
            if (_context.Staff.Any(u => u.Username == "admin"))
            {
                return Content("Database already seeded. Please log in.");
            }

            // Creates a predefined list of staff members with hashed passwords
            var staffList = new List<Staff>
            {
                new Staff { FirstName = "Melard", LastName = "Salapare", Username = "msalapare", Password = BCrypt.Net.BCrypt.HashPassword("1234Admin"), TypeOfUser = "Admin", Status = "Active" },
                new Staff { FirstName = "Admin", LastName = "User", Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("1234Admin"), TypeOfUser = "Admin", Status = "Active" },
                new Staff { FirstName = "Maria", LastName = "Clara", Username = "mclara", Password = BCrypt.Net.BCrypt.HashPassword("staff456"), TypeOfUser = "Librarian", Status = "Active" },
                new Staff { FirstName = "Andres", LastName = "Bonifacio", Username = "abonifacio", Password = BCrypt.Net.BCrypt.HashPassword("staff456"), TypeOfUser = "Librarian", Status = "Active" },
                new Staff { FirstName = "Jose", LastName = "Rizal", Username = "jrizal", Password = BCrypt.Net.BCrypt.HashPassword("student789"), TypeOfUser = "Student Assistant", Status = "Active" },

            };

            _context.Staff.AddRange(staffList);
            _context.SaveChanges();
            return Content("Database Seeded Successfully! Log in with 'admin' / '1234Admin'.");
        }

        #region Staff Management

        // ManageStaff: Retrieves a filtered list of Librarians and Student Assistants for the admin view
        public IActionResult ManageStaff(string searchTerm)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            // Filters out Admins from the management list
            var query = _context.Staff
                .Where(s => s.TypeOfUser == "Librarian" || s.TypeOfUser == "Student Assistant");


            // Applies search filters for Username, ID, or Last Name if a search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Username.Contains(searchTerm) ||
                    s.StaffId.ToString() == searchTerm ||
                    s.LastName.Contains(searchTerm));
            }

            return View("~/Views/Admin/AdminManageStaff.cshtml", query.ToList());
        }

        // AddStaff: Validates and saves a new staff member to the database
        [HttpPost]
        public IActionResult AddStaff([FromBody] Staff newStaff)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            // Ensures required fields are present and the username is unique
            if (string.IsNullOrWhiteSpace(newStaff.FirstName) ||
                string.IsNullOrWhiteSpace(newStaff.LastName) ||
                string.IsNullOrWhiteSpace(newStaff.Username) ||
                string.IsNullOrWhiteSpace(newStaff.Password))
            {
                return Json(new { success = false, message = "All fields are required." });
            }

            if (_context.Staff.Any(s => s.Username == newStaff.Username))
            {
                return Json(new { success = false, message = "Username is already taken." });
            }

            newStaff.Password = BCrypt.Net.BCrypt.HashPassword(newStaff.Password);
            newStaff.Status = "Active";
            _context.Staff.Add(newStaff);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // UpdateStaff: Modifies existing staff details and updates the password if a new one is provided
        [HttpPost]
        public IActionResult UpdateStaff([FromBody] Staff updatedStaff)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            var staff = _context.Staff.Find(updatedStaff.StaffId);
            if (staff == null) return Json(new { success = false, message = "Staff member not found." });

            // VALIDATION: No Empty Fields (Password is optional on update)
            if (string.IsNullOrWhiteSpace(updatedStaff.FirstName) ||
                string.IsNullOrWhiteSpace(updatedStaff.LastName) ||
                string.IsNullOrWhiteSpace(updatedStaff.Username))
            {
                return Json(new { success = false, message = "Name and Username cannot be empty." });
            }

            if (_context.Staff.Any(s => s.Username == updatedStaff.Username && s.StaffId != updatedStaff.StaffId))
            {
                return Json(new { success = false, message = "Username is already taken by another user." });
            }

            // Updates core properties and hashes the new password only if it's not empty
            staff.FirstName = updatedStaff.FirstName;
            staff.LastName = updatedStaff.LastName;
            staff.Username = updatedStaff.Username;
            staff.TypeOfUser = updatedStaff.TypeOfUser;

            if (!string.IsNullOrEmpty(updatedStaff.Password))
            {
                staff.Password = BCrypt.Net.BCrypt.HashPassword(updatedStaff.Password);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }


        // ToggleStatus: Switches a staff member between 'Active' and 'Inactive' (prevents self-deactivation)
        public IActionResult ToggleStatus(int id)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            // Prevent Admin from deactivating their own account
            var currentUserId = HttpContext.Session.GetInt32("StaffId");
            if (id == currentUserId)
            {
                TempData["AlertMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction("ManageStaff");
            }

            var staff = _context.Staff.Find(id);
            if (staff != null)
            {
                staff.Status = (staff.Status == "Active") ? "Inactive" : "Active";
                LogAction($"Changed staff status to {staff.Status} for {staff.Username}", "staff");
                _context.SaveChanges();
                TempData["AlertMessage"] = $"Staff member is now {staff.Status}.";
            }
            return RedirectToAction("ManageStaff");
        }

        // GetStaffDetails: Fetches a single staff member's data for editing (returned as JSON)
        [HttpGet]
        public IActionResult GetStaffDetails(int id)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            var staff = _context.Staff.Find(id);
            if (staff == null) return NotFound();

            return Json(new
            {
                staffId = staff.StaffId,
                firstName = staff.FirstName,
                lastName = staff.LastName,
                username = staff.Username,
                typeOfUser = staff.TypeOfUser
            });
        }

        #endregion

        #region Authentication

        // Login (GET): Checks if a session already exists and redirects the user to their specific dashboard
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                var role = HttpContext.Session.GetString("UserRole");
                if (role == "Patron") return RedirectToAction("PatronDashboard", "Dashboard");
                return RedirectToAction(role.Replace(" ", "") + "Dashboard", "Dashboard");
            }
            return View();
        }

        // Login (POST): Authenticates either Staff (via BCrypt) or Patrons (via ID/LastName) and sets session variables
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // 1. Attempt Staff login with password verification
            var staff = _context.Staff.FirstOrDefault(u => u.Username == username && u.Status == "Active");
            try
            {
                if (staff != null && BCrypt.Net.BCrypt.Verify(password, staff.Password))
                {
                    HttpContext.Session.SetString("UserRole", staff.TypeOfUser);
                    HttpContext.Session.SetString("UserId", staff.StaffId.ToString());
                    HttpContext.Session.SetInt32("StaffId", staff.StaffId);
                    HttpContext.Session.SetString("UserName", staff.FirstName);

                    LogAction("Logged into the system", "staff");
                    _context.SaveChanges();

                    // Set Staff Sessions and Log Action
                    return RedirectToAction(staff.TypeOfUser.Replace(" ", "") + "Dashboard", "Dashboard");
                }
            }
            catch {
                ViewBag.Error = "Invalid ID or Surname.";
                return View();
            }

            // 2. Attempt Patron login (using PatronId and LastName)
            var patron = _context.Patrons.FirstOrDefault(p => p.PatronId == username);
            if (patron != null && patron.LastName.ToLower() == password.ToLower())
            {
                HttpContext.Session.SetString("UserRole", "Patron");
                HttpContext.Session.SetString("UserName", patron.FirstName);
                HttpContext.Session.SetString("UserId", patron.PatronId);
                HttpContext.Session.SetString("UserEmail", patron.Email ?? "");

                LogAction("Logged into Kiosk", "patron");
                _context.SaveChanges();

                // Set Patron Sessions and Log Action
                return RedirectToAction("PatronDashboard", "Dashboard");
            }

            ViewBag.Error = "Invalid ID or Surname.";
            return View();
        }

        // Logout: Logs the logout event, clears the session, and redirects to the login page
        public IActionResult Logout()
        {
            LogAction("Logged out", "session");
            _context.SaveChanges();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Audit Logs & Reports

        // AuditLogs: Collects all system logs, including related Staff and Patron data, for the Admin view
        public IActionResult AuditLogs()
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            var allLogs = _context.AuditLogs
                .Include(l => l.Staff)
                .Include(l => l.Patron)
                .OrderByDescending(l => l.LogDate)
                .ToList();

            ViewBag.StaffLogs = allLogs.Where(l => l.StaffId != null).ToList();
            ViewBag.PatronLogs = allLogs.Where(l => l.PatronId != null).ToList();

            return View("~/Views/Admin/AdminAuditLogs.cshtml");
        }

        // SystemReports: Calculates inventory value, active user counts, and monthly growth percentages for ODDS requests
        public IActionResult SystemReports()
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            // Aggregates financial and user statistics
            var now = DateTime.Now;
            var firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);

            // Calculates performance growth by comparing this month's stats vs. last month's stats
            ViewBag.InventoryValue = _context.Books.Sum(b => (double?)((b.Price ?? 0) - (b.Discount ?? 0))) ?? 0;
            ViewBag.ActivePatrons = _context.Patrons.Count();
            ViewBag.TotalOdds = _context.Odds.Count();
            ViewBag.PendingServices = _context.Services.Count(s => s.RequestStatus == "Pending");

            // Comparison Logic: ODDS Fulfillment
            int thisMonthCount = _context.Odds.Count(o => o.RequestDate >= firstDayThisMonth);
            int lastMonthCount = _context.Odds.Count(o => o.RequestDate >= firstDayLastMonth && o.RequestDate < firstDayThisMonth);

            // Calculate Growth/Decline Percentage
            double diff = thisMonthCount - lastMonthCount;
            ViewBag.OddsGrowth = lastMonthCount == 0 ? 0 : Math.Round((diff / lastMonthCount) * 100, 1);
            ViewBag.IsPositive = diff >= 0;

            // Groups data for charts: Staff fulfillment counts and Material demand types
            ViewBag.StaffPerformance = _context.Odds.Include(o => o.Staff).Where(o => o.RequestStatus == "Fulfilled" && o.Staff != null)
                .GroupBy(o => o.Staff.FirstName).ToDictionary(k => k.Key, v => v.Count());
            ViewBag.MaterialDemand = _context.Odds.GroupBy(o => o.MaterialType).ToDictionary(k => k.Key ?? "Unknown", v => v.Count());

            return View("~/Views/Admin/AdminSystemReports.cshtml");
        }

        #endregion
    }
}