using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // Required for OnActionExecuting
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class BaseController : Controller
    {
        protected readonly LibraryDbContext _context;

        // Constructor: Receives the database context and stores it in a protected field
        // so that all inheriting controllers (like AccountController) can access the DB.
        public BaseController(LibraryDbContext context)
        {
            _context = context;
        }

        // OnActionExecuting: Runs automatically before every action method in the system.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // AUTHENTICATION GUARD: If no user is logged in and they aren't trying 
            // to access the Login/Account pages, force a redirect to the Login screen.
            if (string.IsNullOrEmpty(userId) && controller != "Account")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        // LogAction: Creates a record of user activity (Staff or Patron) in the AuditLogs table.
        protected void LogAction(string action, string table)
        {
            // Get IDs from Session
            int? staffId = HttpContext.Session.GetInt32("StaffId");
            string? patronId = HttpContext.Session.GetString("UserId"); // From your Patron login logic
            string userRole = HttpContext.Session.GetString("UserRole");

            var log = new AuditLog
            {
                // If the user is a Patron, the StaffId will naturally be null, and vice versa.
                StaffId = (userRole != "Patron") ? staffId : null,
                PatronId = (userRole == "Patron") ? patronId : null,
                ActionPerformed = action,
                TableAffected = table,
                LogDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            // SaveChanges is intentionally left out so the calling method can handle the transaction.
        }

     
        protected bool IsAuthorized(params string[] allowedRoles)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(userRole) && allowedRoles.Contains(userRole);
        }
    }
}