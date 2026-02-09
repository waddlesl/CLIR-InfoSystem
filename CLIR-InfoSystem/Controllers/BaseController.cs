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

        public BaseController(LibraryDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This method runs before every single Action in any Controller inheriting from BaseController.
        /// It prevents users from bypassing the Login screen via the URL.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // 1. If no session exists and user is NOT on the Login page, kick them back to Login
            if (string.IsNullOrEmpty(userId) && action != "Login")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Universal Helper to log actions to the audit_logs table.
        /// </summary>
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

        /// <summary>
        /// Helper to check if the current user has the required role.
        /// Usage: if (!IsAuthorized("Admin")) return Unauthorized();
        /// </summary>
        protected bool IsAuthorized(params string[] allowedRoles)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(userRole) && allowedRoles.Contains(userRole);
        }
    }
}