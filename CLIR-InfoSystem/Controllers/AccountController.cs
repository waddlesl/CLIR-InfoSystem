using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; 
using CLIR_InfoSystem.Models;
using CLIR_InfoSystem.Data;
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

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // 1. Staff still need a password for security
            var staff = _context.Staff.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (staff != null)
            {
                HttpContext.Session.SetString("UserRole", staff.TypeOfUser);
                HttpContext.Session.SetString("UserId", staff.StaffId.ToString());
                return RedirectToAction(staff.TypeOfUser + "Dashboard", "Dashboard");
            }

            // 2. Patron Quick Access (ID Only)
            // If password is empty, we check if the username exists in the Patron table
            var patron = _context.Patrons.FirstOrDefault(p => p.PatronId == username);
            if (patron != null)
            {
                HttpContext.Session.SetString("UserRole", "Patron");
                HttpContext.Session.SetString("UserName", patron.FirstName);
                HttpContext.Session.SetString("UserId", patron.PatronId);

                return RedirectToAction("PatronDashboard", "Dashboard");
            }

            ViewBag.Error = "ID not found in system.";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Login");
        }

        public IActionResult ManageStaff(string searchTerm)
        {
            var query = _context.Staff
                .Where(s => s.TypeOfUser != "Admin"); // Hide Admins from this list

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Username.Contains(searchTerm) ||
                                         s.StaffId.ToString() == searchTerm);
            }

            return View(query.ToList());
        }
    }
}