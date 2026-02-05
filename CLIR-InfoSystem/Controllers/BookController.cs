using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly LibraryDbContext _context;

        public BookController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult BookManagement(string searchTerm)
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";

            ViewBag.BookCount = _context.Books.Count();
            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count(b => b.Status == "Borrowed");
            ViewBag.OverdueBookCount = _context.BookBorrowings.Count(b => b.Status == "Overdue");

            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) ||
                                         p.Author.Contains(searchTerm) ||
                                         p.AccessionId == searchTerm);
            }

            var books = query.ToList();
            return View(books);
        }

        [HttpGet]
        public IActionResult AddBook() => View();

        [HttpPost]
        public IActionResult AddBook([FromBody] Book newBook)
        {
            if (newBook == null) return BadRequest();

            bool exists = _context.Books.Any(b => b.AccessionId == newBook.AccessionId);
            if (exists) return BadRequest("Accession ID already exists.");

            if (ModelState.IsValid)
            {
                _context.Books.Add(newBook);
                _context.SaveChanges();
                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        public IActionResult EditBook(string id)
        {
            var book = _context.Books.Find(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        public IActionResult EditBook(Book updatedBook)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Books.Update(updatedBook);
                    _context.SaveChanges();
                    return RedirectToAction("BookManagement", "Book");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Please try again.");
                }
            }
            return View(updatedBook);
        }

        [HttpGet]
        public IActionResult GetBookDetails(string id)
        {
            var book = _context.Books.FirstOrDefault(b => b.AccessionId == id);
            if (book == null) return NotFound();
            return Json(book);
        }

        [HttpPost]
        public IActionResult UpdateBook([FromBody] Book updatedBook)
        {
            if (updatedBook == null) return BadRequest();

            var existingBook = _context.Books.Find(updatedBook.AccessionId);
            if (existingBook == null) return NotFound();

            // Mapping updated fields to the existing entity
            existingBook.Title = updatedBook.Title;
            existingBook.Author = updatedBook.Author;
            existingBook.AvailabilityStatus = updatedBook.AvailabilityStatus;
            existingBook.Edition = updatedBook.Edition;
            existingBook.YearOfPublication = updatedBook.YearOfPublication;
            existingBook.Publisher = updatedBook.Publisher;
            existingBook.Collection = updatedBook.Collection;
            existingBook.LibraryLocation = updatedBook.LibraryLocation;
            existingBook.Supplier = updatedBook.Supplier;
            existingBook.SourcedFrom = updatedBook.SourcedFrom;
            existingBook.Price = updatedBook.Price;
            existingBook.Discount = updatedBook.Discount;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var borrowing = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(b => b.BorrowId == id);
            if (borrowing != null)
            {
                borrowing.Status = "Returned";
                borrowing.ReturnDate = DateTime.Now;

                if (borrowing.Book != null)
                {
                    borrowing.Book.AvailabilityStatus = "Available";
                }

                _context.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult ExtendDueDate(int id, DateTime newDate)
        {
            var borrowing = _context.BookBorrowings.Find(id);
            if (borrowing != null)
            {
                borrowing.DueDate = newDate;
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult ToggleAvailability(string id, string status)
        {
            var book = _context.Books.Find(id);
            if (book != null)
            {
                book.AvailabilityStatus = status;
                _context.SaveChanges();
            }
            return RedirectToAction("BookManagement");
        }



        // 1. Loads the page with available books
        public IActionResult PatronBorrow()
        {
            var availableBooks = _context.Books
                .Where(b => b.AvailabilityStatus == "Available")
                .ToList();
            return View(availableBooks);
        }

        // 2. Handles the actual borrow request
        [HttpPost]
        public IActionResult ProcessBorrow(string patronId, string accessionId)
        {
            var book = _context.Books.Find(accessionId);
            var patronExists = _context.Patrons.Any(p => p.PatronId == patronId);

            // Validation
            if (book == null || book.AvailabilityStatus != "Available")
                return Json(new { success = false, message = "Book is no longer available." });

            if (!patronExists)
                return Json(new { success = false, message = "Invalid Patron ID." });

            // Create Transaction with "Reserved" status
            var borrowRequest = new BookBorrowing
            {
                PatronId = patronId,
                AccessionId = accessionId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                Status = "Reserved",               // Matches your SQL ENUM
                StaffId = null                     // Pending approval
            };

            // Update Book status so it's hidden from the "Available" list
            book.AvailabilityStatus = "Reserved";

            _context.BookBorrowings.Add(borrowRequest);
            _context.SaveChanges();

            return Json(new { success = true, message = "Request submitted! Please proceed to the counter." });
        }
    }
}