using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryDbContext _context;

        public AccountController(LibraryDbContext context)
        {
            _context = context;
        }

        #region Authentication

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // 1. Staff Login - Checks for Username, Password, and Active status
            var staff = _context.Staff.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password &&
                u.Status == "Active");

            if (staff != null)
            {
                HttpContext.Session.SetString("UserRole", staff.TypeOfUser);
                HttpContext.Session.SetString("UserId", staff.StaffId.ToString());
                HttpContext.Session.SetString("UserName", staff.FirstName);

                // Dynamically routes based on role (e.g., LibrarianDashboard, AdminDashboard)
                string dashboardAction = staff.TypeOfUser.Replace(" ", "") + "Dashboard";
                return RedirectToAction(dashboardAction, "Dashboard");
            }

            // 2. Patron Login - Includes Dept/Prog objects to avoid null refs in session
            var patron = _context.Patrons
                .Include(p => p.Department)
                .Include(p => p.Program)
                .FirstOrDefault(p => p.PatronId == username);

            // Note: Since patrons usually don't have a 'password' field in your model, 
            // this currently acts as an ID-only login. 
            if (patron != null)
            {
                HttpContext.Session.SetString("UserRole", "Patron");
                HttpContext.Session.SetString("UserName", patron.FirstName);
                HttpContext.Session.SetString("UserId", patron.PatronId);
                HttpContext.Session.SetString("UserDept", patron.Department?.DeptName ?? "N/A");

                return RedirectToAction("PatronDashboard", "Dashboard");
            }

            ViewBag.Error = "Invalid Credentials or Inactive Account.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Staff Management

        public IActionResult ManageStaff(string searchTerm)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Unauthorized();

            var query = _context.Staff
                .Where(s => s.TypeOfUser == "Librarian" || s.TypeOfUser == "Student Assistant");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s =>
                    s.Username.Contains(searchTerm) ||
                    s.StaffId.ToString() == searchTerm ||
                    s.LastName.Contains(searchTerm));
            }

            return View(query.ToList());
        }

        [HttpGet]
        public IActionResult GetStaffDetails(int id)
        {
            var staff = _context.Staff.Find(id);
            if (staff == null) return NotFound();
            return Json(staff);
        }

        [HttpPost]
        public IActionResult AddStaff([FromBody] Staff newStaff)
        {
            if (newStaff == null) return Json(new { success = false });

            // Set default status if not provided
            if (string.IsNullOrEmpty(newStaff.Status)) newStaff.Status = "Active";

            _context.Staff.Add(newStaff);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateStaff([FromBody] Staff updatedStaff)
        {
            if (updatedStaff == null) return Json(new { success = false, message = "No data received" });

            var staff = _context.Staff.Find(updatedStaff.StaffId);
            if (staff == null) return Json(new { success = false, message = "Staff member not found" });

            // Update basic fields
            staff.FirstName = updatedStaff.FirstName;
            staff.LastName = updatedStaff.LastName;
            staff.Username = updatedStaff.Username;
            staff.TypeOfUser = updatedStaff.TypeOfUser;
            // Optionally update password if provided
            if (!string.IsNullOrEmpty(updatedStaff.Password)) staff.Password = updatedStaff.Password;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult ToggleStatus(int id)
        {
            var staff = _context.Staff.Find(id);
            if (staff != null)
            {
                staff.Status = (staff.Status == "Active") ? "Inactive" : "Active";
                _context.SaveChanges();
                TempData["AlertMessage"] = $"Staff member is now {staff.Status}.";
            }
            return RedirectToAction("ManageStaff");
        }

        #endregion
    }
}