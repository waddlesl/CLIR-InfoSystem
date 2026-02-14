using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(LibraryDbContext context) : base(context) { }

        #region Authentication

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, skip the login page
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                var role = HttpContext.Session.GetString("UserRole");
                if (role == "Patron") return RedirectToAction("PatronDashboard", "Dashboard");
                return RedirectToAction(role.Replace(" ", "") + "Dashboard", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // 1. Staff Login
            var staff = _context.Staff.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password &&
                u.Status == "Active");

            if (staff != null)
            {
                HttpContext.Session.SetString("UserRole", staff.TypeOfUser);
                HttpContext.Session.SetString("UserId", staff.StaffId.ToString());
                HttpContext.Session.SetInt32("StaffId", staff.StaffId);
                HttpContext.Session.SetString("UserName", staff.FirstName);

                LogAction("Logged into the system", "staff");
                _context.SaveChanges();

                string dashboardAction = staff.TypeOfUser.Replace(" ", "") + "Dashboard";
                return RedirectToAction(dashboardAction, "Dashboard");
            }

            // 2. Patron Login (ID + Last Name)
            var patron = _context.Patrons
                .Include(p => p.Department)
                .Include(p => p.Program)
                .FirstOrDefault(p => p.PatronId == username);

            if (patron != null && patron.LastName.ToLower() == password.ToLower())
            {
                HttpContext.Session.SetString("UserRole", "Patron");
                HttpContext.Session.SetString("UserName", patron.FirstName);
                HttpContext.Session.SetString("UserId", patron.PatronId);

                LogAction("Logged into Kiosk", "patron");
                _context.SaveChanges();

                return RedirectToAction("PatronDashboard", "Dashboard");
            }

            ViewBag.Error = "Invalid ID or Surname. Please try again.";
            return View();
        }

        public IActionResult Logout()
        {
            LogAction("Logged out", "session");
            _context.SaveChanges();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Staff Management 

        public IActionResult ManageStaff(string searchTerm)
        {
            // Use the BaseController helper to restrict access
            if (!IsAuthorized("Admin")) return Unauthorized();

            var query = _context.Staff
                .Where(s => s.TypeOfUser == "Librarian" || s.TypeOfUser == "Student Assistant");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Username.Contains(searchTerm) ||
                    s.StaffId.ToString() == searchTerm ||
                    s.LastName.Contains(searchTerm));
            }

            return View("~/Views/Admin/AdminManageStaff.cshtml",query.ToList());
        }

        [HttpPost]
        public IActionResult AddStaff([FromBody] Staff newStaff)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();
            if (newStaff == null) return Json(new { success = false });

            if (string.IsNullOrEmpty(newStaff.Status)) newStaff.Status = "Active";

            _context.Staff.Add(newStaff);
            LogAction($"Added new staff: {newStaff.Username}", "staff");
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateStaff([FromBody] Staff updatedStaff)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            var staff = _context.Staff.Find(updatedStaff.StaffId);
            if (staff == null) return Json(new { success = false, message = "Not found" });

            staff.FirstName = updatedStaff.FirstName;
            staff.LastName = updatedStaff.LastName;
            staff.Username = updatedStaff.Username;
            staff.TypeOfUser = updatedStaff.TypeOfUser;
            if (!string.IsNullOrEmpty(updatedStaff.Password)) staff.Password = updatedStaff.Password;

            LogAction($"Updated staff profile: {staff.Username}", "staff");
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult ToggleStatus(int id)
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

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

        #region Audit Logs & System Reports

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

        public IActionResult SystemReports()
        {
            if (!IsAuthorized("Admin")) return Unauthorized();

            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.ActivePatrons = _context.Patrons.Count();
            ViewBag.TotalOdds = _context.Odds.Count();
            ViewBag.InventoryValue = _context.Books.Sum(b => (b.Price ?? 0) - (b.Discount ?? 0));

            ViewBag.StaffPerformance = _context.Odds
                .Include(o => o.Staff)
                .Where(o => o.RequestStatus == "Fulfilled" && o.Staff != null)
                .GroupBy(o => o.Staff.FirstName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionary(k => k.Name, v => v.Count);

            return View("~/Views/Admin/AdminSystemReports.cshtml");
        }

        #endregion
    }
}