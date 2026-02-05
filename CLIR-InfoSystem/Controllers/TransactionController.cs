using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CLIR_InfoSystem.Controllers
{
    public class TransactionController : Controller
    {
        private readonly LibraryDbContext _context;

        public TransactionController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var myLoans = _context.BookBorrowings
                .Include(b => b.Book)
                .Where(b => b.PatronId == userId && b.Status != "Returned" && b.Status != "Denied")
                .OrderByDescending(b => b.BorrowDate)
                .ToList();

            return View(myLoans);
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
            _context.SaveChanges();

            TempData["Success"] = "Book requested successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult BorrowersHistory()
        {
            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();
            var results = query.OrderByDescending(b => b.BorrowDate).ToList() ?? new List<BookBorrowing>();

            return View(results);
 
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

            return View(results);
            
        }

        [HttpPost]
        public IActionResult ApproveRequest(int id)
        {
            var request = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(r => r.BorrowId == id);
            if (request != null && request.Book != null)
            {
                request.Status = "Borrowed";
                request.BorrowDate = DateTime.Now;
                request.DueDate = DateTime.Now.AddDays(7);
                request.Book.AvailabilityStatus = "Borrowed"; // Update the book itself

                _context.SaveChanges();
            }
            return RedirectToAction("BookBorrowers");
        }

        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var request = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(r => r.BorrowId == id);
            if (request != null && request.Book != null)
            {
                request.Status = "Returned";
                request.ReturnDate = DateTime.Now;
                request.Book.AvailabilityStatus = "Available"; // Make it available for others

                _context.SaveChanges();
                TempData["Success"] = "Book returned successfully.";
            }
            return RedirectToAction("BookBorrowers");
        }

        [HttpPost]
        public IActionResult RejectRequest(int id)
        {
            var request = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(r => r.BorrowId == id);
            if (request != null)
            {
                request.Status = "Denied";
                if (request.Book != null) request.Book.AvailabilityStatus = "Available";
                _context.SaveChanges();
            }
            return RedirectToAction("BookBorrowers");
        }
    }
}