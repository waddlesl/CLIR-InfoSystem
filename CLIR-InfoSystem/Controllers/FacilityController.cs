using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLIR_InfoSystem.Controllers
{
    public class FacilityController : Controller
    {
        private readonly LibraryDbContext _context;

        public FacilityController(LibraryDbContext context)
        {
            _context = context;
        }

        // --- PATRON ACTIONS ---

        public IActionResult BookASeat()
        {
            var currentTime = DateTime.Now.TimeOfDay;

            // Initial load: Only show slots that haven't passed yet for today
            var availableSlots = _context.TimeSlots
                .Where(s => s.StartTime >= currentTime)
                .ToList();

            ViewBag.TimeSlots = availableSlots;
            return View();
        }

        public JsonResult GetAvailableSlots(DateTime selectedDate)
        {
            var slots = _context.TimeSlots.AsQueryable();

            if (selectedDate.Date == DateTime.Today)
            {
                var now = DateTime.Now.TimeOfDay;
                slots = slots.Where(s => s.StartTime >= now);
            }

            return Json(slots.ToList());
        }

        [HttpGet]
        public JsonResult GetAvailableSeats(string building, int slotId, DateTime date)
        {
            var occupiedSeatIds = _context.SeatBookings
                .Where(b => b.BookingDate.Date == date.Date &&
                            b.SlotId == slotId &&
                            (b.Status == "Reserved" || b.Status == "Completed"))
                .Select(b => b.SeatId).ToList();

            var availableSeats = _context.LibrarySeats
                .Where(s => s.Building == building && !occupiedSeatIds.Contains(s.Id))
                .Select(s => new { id = s.Id, name = $"{s.SeatName} ({s.SeatType})" })
                .ToList();

            return Json(availableSeats);
        }

        [HttpPost]
        public IActionResult ConfirmBooking(SeatBooking booking)
        {
            string? loggedInId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(loggedInId)) return RedirectToAction("Login", "Account");

            booking.PatronId = loggedInId;
            booking.Status = "Reserved";

            // 🚩 FORCE REMOVE these from validation or IsValid will stay FALSE
            ModelState.Remove("Patron");
            ModelState.Remove("LibrarySeat"); // Add this
            ModelState.Remove("TimeSlot");
            ModelState.Remove("PatronId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.SeatBookings.Add(booking);
                _context.SaveChanges();
                TempData["Success"] = "Reservation saved successfully!";
                return RedirectToAction("BookASeat");
            }

            // If it fails, reload the data for the dropdowns
            ViewBag.TimeSlots = _context.TimeSlots.ToList();
            return View("BookASeat", booking);
        }

        // --- LIBRARIAN / STAFF ACTIONS ---

        public IActionResult ManageBookings()
        {
            var activeBookings = _context.SeatBookings
                .Include(b => b.TimeSlot)
                .Include(b => b.Patron) // Add this line
                .Where(b => b.Status == "Reserved")
                .ToList();

            return View(activeBookings);
        }

        [HttpPost]
        public IActionResult CheckIn(int bookingId)
        {
            var booking = _context.SeatBookings.Find(bookingId);
            if (booking != null)
            {
                booking.Status = "Completed";
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
                _context.SaveChanges();
                TempData["Info"] = "Booking has been cancelled.";
            }
            return RedirectToAction("ManageBookings");
        }
    }
}