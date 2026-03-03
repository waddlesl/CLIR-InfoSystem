using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;

namespace CLIR_InfoSystem.Controllers
{
    public class ServiceController : BaseController
    {
        public ServiceController(LibraryDbContext context) : base(context)
        {
        }

        // --- ODDS MANAGEMENT ---

        public IActionResult ManageODDS()
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffManageODDS.cshtml", odds);
        }
        
        public IActionResult ODDSHistory()
        {
            var odds = _context.Odds
                .Include(s => s.Patron)
                .ThenInclude(p => p.Department)
                .Include(s => s.Book)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffODDSHistory.cshtml", odds);
        }

        public IActionResult ManageServices()
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var today = DateTime.Now;
            var expiredRequest = _context.Services
                .Where(b => b.RequestStatus == "Approved" && today > b.RequestDate.AddDays(90))
                .ToList();

            if (expiredRequest.Any())
            {
                foreach (var service in expiredRequest)
                {
                    service.RequestStatus = "Expired";
                    // AUDIT LOG (Internal System Action)
                    LogAction($"Service Request #{service.ServiceId} automatically expired.", "System");
                }
                _context.SaveChanges();
            }

            var services = _context.Services
                .Include(s => s.Patron)
                    .ThenInclude(p => p.Department)
                .Include(s => s.Staff)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffManageServices.cshtml", services);
        }

        public IActionResult ServicesHistory()
        {
            var today = DateTime.Now;
            var expiredRequest = _context.Services
                .Where(b => b.RequestStatus == "Approved" && today > b.RequestDate.AddDays(90))
                .ToList();

            if (expiredRequest.Any())
            {
                foreach (var service in expiredRequest)
                {
                    service.RequestStatus = "Expired";
                    // AUDIT LOG (Internal System Action)
                    LogAction($"Service Request #{service.ServiceId} automatically expired.", "System");
                }
                _context.SaveChanges();
            }

            var services = _context.Services
                .Include(s => s.Patron)
                    .ThenInclude(p => p.Department)
                .Include(s => s.Staff)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View("~/Views/Staff/StaffServicesHistory.cshtml", services);
        }


        [HttpGet]
        public IActionResult UpdateOddsStatus(int id, string status)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.Odds.Find(id);
            if(request == null)
            {
                TempData["Error"] = "Service request not found.";
                return RedirectToAction("ManageODDS"); 
            }


            request.RequestStatus = status;
            if (status == "Fulfilled")
            {
                request.DateOfAccessProvided = DateTime.Now;
            }

            // AUDIT LOG
            LogAction($"Updated ODDS Request #{id} status to: {status}", "System");
            _context.SaveChanges();
            return RedirectToAction("ManageODDS");
        }

        // --- GRAMMARLY & TURNITIN MANAGEMENT ---

        public IActionResult ManageServiceRequests()
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var requests = _context.Services
                .Include(s => s.Patron)
                .OrderByDescending(s => s.RequestDate)
                .ToList();

            return View(requests);
        }

        [HttpGet]
        public IActionResult UpdateServiceStatus(int requestId, string status)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();

            
            var request = _context.Services
                .Include(s => s.Patron)
                .FirstOrDefault(s => s.ServiceId == requestId);

            if (request == null) return NotFound();

            request.RequestStatus = status;

            // Trigger real email on Approval
            if (status == "Approved" && !string.IsNullOrEmpty(request.Patron?.Email))
            {
                SendServiceEmail(request.Patron.Email, request.ServiceType, request.Patron.FirstName);
            }

            LogAction($"Updated Service Request #{requestId} ({request.ServiceType}) to {status}", "Services");
            _context.SaveChanges();

            return RedirectToAction("ManageServices");
        }

        private void SendServiceEmail(string toEmail, string serviceName, string patronName)
        {
            try
            {
                var senderEmail = "clirnotifications@gmail.com";
                var appPassword = "zbdwacfjpfvtolpe"; 

                var mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "CLIR Library System");
                mail.To.Add(new MailAddress(toEmail));
                mail.Subject = $"Access Approved: {serviceName}";
                mail.IsBodyHtml = true;
                mail.Body = $"<p>Hello {patronName}, your request for {serviceName} is approved!</p>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, appPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                LogAction($"Email Error: {ex.Message}", "System");
            }
        }

    }
}