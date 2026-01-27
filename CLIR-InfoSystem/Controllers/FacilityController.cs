using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CLIR_InfoSystem.Controllers
{
    public class FacilityController : Controller
    {
        private readonly LibraryDbContext _context;

        public FacilityController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult BookASeat()
        {
            // Get the ID from the logged-in user identity or session
            string loggedInPatronId = User.Identity.Name;

            var model = new SeatBooking
            {
                PatronId = loggedInPatronId
            };

            ViewBag.TimeSlots = _context.TimeSlots.ToList();
            return View(model);
        }

        [HttpGet]
        public JsonResult GetAvailableSeats(string building, int slotId, DateTime date)
        {
            var occupiedSeatIds = _context.SeatBookings
                .Where(b => b.BookingDate.Date == date.Date &&
                            b.SlotId == slotId &&
                            b.Status == "Reserved")
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
            // 1. Pull using the EXACT key from AccountController
            string loggedInId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(loggedInId))
            {
                // If session expired, send them back to login
                return RedirectToAction("Login", "Account");
            }

            // 2. Assign the ID and clear validation
            booking.PatronId = loggedInId;
            booking.Status = "Reserved";
            ModelState.Remove("PatronId");

            if (ModelState.IsValid)
            {
                _context.SeatBookings.Add(booking);
                _context.SaveChanges();
                TempData["Success"] = "Reservation saved successfully!";
                return RedirectToAction("BookASeat");
            }

            ViewBag.TimeSlots = _context.TimeSlots.ToList();
            return View("BookASeat", booking);
        }
    }



}
