using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class TransactionController : BaseController
    {
        public TransactionController(LibraryDbContext context) : base(context) { }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var myLoans = _context.BookBorrowings
                .Include(b => b.Book)
                .Where(b => b.PatronId == userId && b.Status != "Returned" && b.Status != "Denied")
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            return View("~/Views/Patron/PatronMyBorrowings.cshtml",myLoans);
        }

        [HttpPost]
        public IActionResult RequestBook(string accessionId)
        {
            var patronId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(patronId)) return RedirectToAction("Login", "Account");

            var book = _context.Books.Find(accessionId);
            if (book == null || book.AvailabilityStatus != "Available")
            {
                TempData["Error"] = "Book is currently unavailable.";
                return RedirectToAction("Index", "Book");
            }

            var request = new BookBorrowing
            {
                PatronId = patronId,
                AccessionId = accessionId,
                BorrowDate = DateTime.Now,
                Status = "Reserved"
            };

            book.AvailabilityStatus = "Reserved";
            _context.BookBorrowings.Add(request);


            // AUDIT LOG
            LogAction($"Requested book: {book.Title} (Acc# {accessionId})", "Transactions");
            _context.SaveChanges();
            TempData["Success"] = "Book requested successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult BorrowersHistory()
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();
            var results = query.OrderByDescending(b => b.BorrowDate).ToList() ?? new List<BookBorrowing>();

            return View("~/Views/Staff/StaffBorrowersHistory.cshtml", results);
        }

        public IActionResult BookBorrowers(string searchTerm)
        {
            var today = DateTime.Now;
            var overdueBooks = _context.BookBorrowings
                .Where(b => b.Status == "Borrowed" && b.DueDate < today)
                .ToList();

            if (overdueBooks.Any())
            {
                foreach (var loan in overdueBooks)
                {
                    loan.Status = "Overdue";
                }
                _context.SaveChanges();
            }

            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count(b => b.Status == "Borrowed");
            ViewBag.OverdueBookCount = _context.BookBorrowings.Count(b => b.Status == "Overdue");

            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Patron.FirstName.Contains(searchTerm) ||
                                         p.Patron.LastName.Contains(searchTerm) ||
                                         p.BorrowId.ToString().Contains(searchTerm) ||
                                         (p.Book != null && p.Book.Title.Contains(searchTerm)));
            }

            var results = query.OrderByDescending(b => b.BorrowDate).ToList() ?? new List<BookBorrowing>();

            return View("~/Views/Staff/StaffBookBorrowers.cshtml", results);


        }

        public IActionResult BookBorrowersRequest(string searchTerm)
        {
            var today = DateTime.Now;
            var overdueBooks = _context.BookBorrowings
                .Where(b => b.Status == "Borrowed" && b.DueDate < today)
                .ToList();

            if (overdueBooks.Any())
            {
                foreach (var loan in overdueBooks)
                {
                    loan.Status = "Overdue";
                }
                _context.SaveChanges();
            }

            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count(b => b.Status == "Borrowed");
            ViewBag.OverdueBookCount = _context.BookBorrowings.Count(b => b.Status == "Overdue");

            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Patron.FirstName.Contains(searchTerm) ||
                                         p.Patron.LastName.Contains(searchTerm) ||
                                         p.BorrowId.ToString().Contains(searchTerm) ||
                                         (p.Book != null && p.Book.Title.Contains(searchTerm)));
            }

            var results = query.OrderByDescending(b => b.BorrowDate).ToList() ?? new List<BookBorrowing>();

            return View("~/Views/Staff/StaffBookBorrowersRequest.cshtml", results);

        }

        [HttpGet]
        public IActionResult ToggleAvailability(int id, string status)
        {

            if (status == "confirm")
            {
                return RedirectToAction("ApproveRequest", new { id = id });
            }
            return RedirectToAction("DenyRequest", new { id = id });
        }

        [HttpGet]
        public IActionResult ApproveRequest(int id)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.BookBorrowings.Include(b => b.Book).Include(b => b.Patron).FirstOrDefault(r => r.BorrowId == id);
            if (request != null)
            {
                request.Status = "Borrowed";
                request.Book.AvailabilityStatus = "Borrowed";
                request.BorrowDate = DateTime.Now;
                request.DueDate = DateTime.Now.AddDays(7);
                request.Book.AvailabilityStatus = "Borrowed";



                // AUDIT LOG
                LogAction($"Approved borrowing request #{id} for {request.Patron?.FirstName} {request.Patron?.LastName}", "Transactions");
                _context.SaveChanges();
                return RedirectToAction("BookBorrowers");
            }
            else {
            }
            return RedirectToAction("BookBorrowersRequest");
        }

        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var request = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(r => r.BorrowId == id);
            if (request != null && request.Book != null)
            {
                request.Status = "Returned";
                request.Book.AvailabilityStatus = "Available";
                request.ReturnDate = DateTime.Now;
                request.Book.AvailabilityStatus = "Available";



                // AUDIT LOG
                LogAction($"Processed return for book: {request.Book.Title} (BorrowID: {id})", "Transactions");
                _context.SaveChanges();
                TempData["Success"] = "Book returned successfully.";
            }
            return RedirectToAction("BookBorrowers");
        }

        [HttpGet]
        public IActionResult DenyRequest(int id)
        {
            var request = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(r => r.BorrowId == id);
            if (request != null)
            {
                request.Status = "Denied";
                if (request.Book != null) request.Book.AvailabilityStatus = "Available";

                // AUDIT LOG
                LogAction($"Rejected borrowing request #{id}", "Transactions");
                _context.SaveChanges();
            }
            return RedirectToAction("BookBorrowersRequest");
        }
    }
}