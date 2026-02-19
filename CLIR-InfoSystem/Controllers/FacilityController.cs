using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    // Inherit from BaseController to use shared _context and LogAction helper
    public class FacilityController : BaseController
    {
        public FacilityController(LibraryDbContext context) : base(context) { }

        // --- SEAT BOOKING LOGIC ---

        public IActionResult BookASeat()
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");
            ViewBag.AlreadyBooked = _context.SeatBookings
                .Any(b => b.PatronId == loggedInId && b.Status == "Reserved");

            var currentTime = DateTime.Now.TimeOfDay;
            ViewBag.TimeSlots = _context.TimeSlots.Where(s => s.StartTime >= currentTime).ToList();
            return View("~/Views/Patron/PatronBookASeat.cshtml");
        }

        [HttpPost]
        public IActionResult ConfirmBooking(SeatBooking booking)
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");

            bool isAlreadyTaken = _context.SeatBookings.Any(s =>
                s.SeatId == booking.SeatId &&
                s.SlotId == booking.SlotId &&
                s.BookingDate.Date == booking.BookingDate.Date &&
                s.Status != "Cancelled");

            if (isAlreadyTaken)
            {
                return Json(new { success = false, message = "This seat was just reserved by someone else!" });
            }

            booking.PatronId = loggedInId;
            booking.Status = "Reserved";

            ModelState.Remove("Patron");
            ModelState.Remove("LibrarySeat");
            ModelState.Remove("TimeSlot");
            ModelState.Remove("PatronId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.SeatBookings.Add(booking);

                // AUDIT: Log seat reservation
                LogAction($"Reserved Seat ID: {booking.SeatId} for {booking.BookingDate.ToShortDateString()}", "book_a_seat");

                _context.SaveChanges();
                return Json(new { success = true, message = "Seat reserved successfully!" });
            }
            return Json(new { success = false, message = "Selection error. Please try again." });
        }

        // --- LIBRARIAN CONSULTATION LOGIC ---

        [HttpGet]
        public IActionResult BookALibrarian()
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");
            ViewBag.HasActiveRequest = _context.BookALibrarians.Any(b =>
                b.PatronId == loggedInId &&
                (b.Status == "Pending" || b.Status == "Approved"));

            ViewBag.Librarians = _context.Staff.ToList();
            return View("~/Views/Patron/PatronBookALibrarian.cshtml");
        }

        public IActionResult ManageBookings(DateTime? selectedDate, string? building)
        {
            var query = _context.SeatBookings
                .Include(b => b.TimeSlot)
                .AsQueryable();
            if (selectedDate.HasValue)
            {
                query = query.Where(b => b.BookingDate.Date == selectedDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(building))
            {
                if (building == "Einstein") {
                    query = query.Where(b => b.SeatId > 14);
                }
                else
                {
                    query = query.Where(b => b.SeatId < 15);
                }

            }

            // Pass filters back to the view to keep the dropdowns synced
            ViewBag.SelectedDate = selectedDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedBuilding = building;

            var activeBookings = query.OrderByDescending(b => b.BookingDate).ToList();
            return View("~/Views/Staff/StaffManageBookings.cshtml", activeBookings);
        }


        
        public IActionResult HistoryBookaLibrarian()
        {
            var today = DateTime.Now;
            var completedRequest = _context.BookALibrarians
                .Where(b => b.Status == "Approved" && today > b.BookingDate)
                .ToList();

            if (completedRequest.Any())
            {
                foreach (var service in completedRequest)
                {
                    service.Status = "Completed";
                }
                _context.SaveChanges();
            }

            string userId = HttpContext.Session.GetString("UserId");
            var librarian = _context.BookALibrarians
                .Include(s => s.Patron)
                    .ThenInclude(p => p.Department)
                .Include(s => s.Staff)
                .Where(s => s.Staff.StaffId == Convert.ToInt32(userId))
                .OrderByDescending(o => o.BookingDate)
                .ToList();

            return View("~/Views/Staff/StaffHistoryBookaLibrarian.cshtml", librarian);
        }
        public IActionResult ManageBookaLibrarian()
        {
            var today = DateTime.Now;
            var completedRequest = _context.BookALibrarians
                .Where(b => b.Status == "Approved" && today > b.BookingDate)
                .ToList();

            if (completedRequest.Any())
            {
                foreach (var service in completedRequest)
                {
                    service.Status = "Completed";
                }
                _context.SaveChanges();
            }

            string userId = HttpContext.Session.GetString("UserId");
            var librarian = _context.BookALibrarians
                .Include(s => s.Patron)
                    .ThenInclude(p => p.Department)
                .Include(s => s.Staff)
                .Where(s => s.Staff.StaffId == Convert.ToInt32(userId))
                .OrderByDescending(o => o.BookingDate)
                .ToList();

            return View("~/Views/Staff/StaffManageBookaLibrarian.cshtml", librarian);
        }

        [HttpPost]
        public IActionResult SubmitBooking(BookALibrarian model)
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");

            var existingRequest = _context.BookALibrarians.FirstOrDefault(b =>
                b.PatronId == loggedInId &&
                (b.Status == "Pending" || b.Status == "Approved"));

            if (existingRequest != null)
            {
                return Json(new { success = false, message = "You already have an active request." });
            }

            var hour = model.BookingDate.Hour;
            var day = model.BookingDate.DayOfWeek;

            if (hour < 8 || hour >= 17 || day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
            {
                return Json(new { success = false, message = "Please select a weekday between 8:00 AM and 5:00 PM." });
            }

            model.PatronId = loggedInId;
            model.Status = "Pending";
            model.SchoolYear = "2025-2026";
            if (model.StaffId == 0) model.StaffId = null;

            ModelState.Remove("Patron");
            ModelState.Remove("Staff");
            ModelState.Remove("SchoolYear");
            ModelState.Remove("Status");
            ModelState.Remove("PatronId");

            if (ModelState.IsValid)
            {
                _context.BookALibrarians.Add(model);

                // AUDIT: Log librarian request
                LogAction($"Requested librarian consultation for {model.BookingDate}", "book_a_librarian");

                _context.SaveChanges();
                return Json(new { success = true, message = "Request sent successfully!" });
            }

            return Json(new { success = false, message = "Please check your inputs." });
        }

        // AJAX HELPERS
        public JsonResult GetAvailableSlots(DateTime selectedDate)
        {
            var slots = _context.TimeSlots.AsQueryable();
            if (selectedDate.Date == DateTime.Today)
            {
                var now = DateTime.Now.TimeOfDay;
                slots = slots.Where(s => s.StartTime >= now);
            }
            return Json(slots.Select(s => new { slotId = s.SlotId, displayText = s.DisplayText }).ToList());
        }

        [HttpGet]
        public JsonResult GetAvailableSeats(string building, int slotId, DateTime date)
        {
            var occupied = _context.SeatBookings
                .Where(b => b.BookingDate.Date == date.Date && b.SlotId == slotId && b.Status != "Cancelled")
                .Select(b => b.SeatId).ToList();

            var seats = _context.LibrarySeats
                .Where(s => s.Building == building && !occupied.Contains(s.Id))
                .Select(s => new { id = s.Id, name = $"{s.SeatName} ({s.SeatType})" }).ToList();

            return Json(seats);
        }

        [HttpPost]
        public IActionResult CheckIn(int bookingId)
        {
            var booking = _context.SeatBookings.Find(bookingId);
            if (booking != null)
            {
                booking.Status = "Completed";
                LogAction($"Checked in patron for Seat Booking #{bookingId}", "book_a_seat");
                _context.SaveChanges();
                TempData["Success"] = "Patron checked in successfully.";
            }
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            var booking = _context.SeatBookings.Find(bookingId);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                LogAction($"Cancelled Seat Booking #{bookingId}", "book_a_seat");
                _context.SaveChanges();
                TempData["Info"] = "Booking has been cancelled.";
            }
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public IActionResult LibrarianCheckIn(int sessionId)
        {
            var booking = _context.BookALibrarians.Find(sessionId);
            if (booking != null)
            {
                booking.Status = "Approved";
                LogAction($"Approved Librarian Consultation #{sessionId}", "book_a_librarian");
                _context.SaveChanges();
                TempData["Success"] = "Patron checked in successfully.";
            }
            return RedirectToAction("ManageBookaLibrarian");
        }

        public IActionResult LibrarianComplete(int sessionId)
        {
            var booking = _context.BookALibrarians.Find(sessionId);
            if (booking != null)
            {
                booking.Status = "Completed";
                LogAction($"Completed Librarian Consultation #{sessionId}", "book_a_librarian");
                _context.SaveChanges();
                TempData["Success"] = "Patron checked in successfully.";
            }
            return RedirectToAction("ManageBookaLibrarian");
        }

        [HttpPost]
        public IActionResult CancelLibrarianBooking(int sessionId)
        {
            var booking = _context.BookALibrarians.Find(sessionId);
            if (booking != null)
            {
                booking.Status = "Cancelled";
                LogAction($"Cancelled Librarian Consultation #{sessionId}", "book_a_librarian");
                _context.SaveChanges();
                TempData["Info"] = "Booking has been cancelled.";
            }
            return RedirectToAction("ManageBookaLibrarian");
        }
    }
}