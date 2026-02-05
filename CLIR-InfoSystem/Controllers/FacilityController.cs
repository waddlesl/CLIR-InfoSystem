using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class FacilityController : Controller
    {
        private readonly LibraryDbContext _context;

        public FacilityController(LibraryDbContext context)
        {
            _context = context;
        }

        // --- SEAT BOOKING LOGIC ---

        public IActionResult BookASeat()
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");
            ViewBag.AlreadyBooked = _context.SeatBookings
                .Any(b => b.PatronId == loggedInId && b.Status == "Reserved");

            var currentTime = DateTime.Now.TimeOfDay;
            ViewBag.TimeSlots = _context.TimeSlots.Where(s => s.StartTime >= currentTime).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmBooking(SeatBooking booking)
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");

            // 1. PREVENT THE CRASH: Manual check for the UNIQUE constraint
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

            // 2. FIX "BOOKING FAILED": Remove navigation properties from validation
            ModelState.Remove("Patron");
            ModelState.Remove("LibrarySeat");
            ModelState.Remove("TimeSlot");
            ModelState.Remove("PatronId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.SeatBookings.Add(booking);
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
            // Check if user is blocked from booking
            ViewBag.HasActiveRequest = _context.BookALibrarians.Any(b =>
                b.PatronId == loggedInId &&
                (b.Status == "Pending" || b.Status == "Approved"));

            ViewBag.Librarians = _context.Staff.ToList();
            return View();
        }
        public IActionResult ManageBookings()
        {
            var activeBookings = _context.SeatBookings
                .Include(b => b.TimeSlot)
                //.Include(b => b.Patron) 
                //.Where(b => b.Status == "Reserved")
                .ToList();

            return View(activeBookings);

        }


        [HttpPost]
        public IActionResult SubmitBooking(BookALibrarian model)
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");

            // 1. DUPLICATE CHECK: Check if the user already has an active or pending request
            var existingRequest = _context.BookALibrarians.FirstOrDefault(b =>
                b.PatronId == loggedInId &&
                (b.Status == "Pending" || b.Status == "Approved"));

            if (existingRequest != null)
            {
                return Json(new
                {
                    success = false,
                    message = "You already have an active or pending consultation request."
                });
            }

            // Time Validation: Check if between 8am and 5pm
            var hour = model.BookingDate.Hour;
            var day = model.BookingDate.DayOfWeek;

            if (hour < 8 || hour >= 17 || day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
            {
                return Json(new { success = false, message = "Please select a weekday between 8:00 AM and 5:00 PM." });
            }


            // 2. Set defaults
            model.PatronId = loggedInId;
            model.Status = "Pending";
            model.SchoolYear = "2025-2026";
            if (model.StaffId == 0) model.StaffId = null;

            // 3. Validation Cleanup
            ModelState.Remove("Patron");
            ModelState.Remove("Staff");
            ModelState.Remove("SchoolYear");
            ModelState.Remove("Status");
            ModelState.Remove("PatronId");

            if (ModelState.IsValid)
            {
                _context.BookALibrarians.Add(model);
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
    }
}