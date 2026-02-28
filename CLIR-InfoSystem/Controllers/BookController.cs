using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class BookController : BaseController
    {
        public BookController(LibraryDbContext context) : base(context) { }

        public IActionResult BookManagement(string searchTerm)
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.IsStudentAssistant = role == "Student Assistant";

            ViewBag.BookCount = _context.Books.Count();
            ViewBag.AvailableBookCount = _context.Books.Count(b => b.AvailabilityStatus == "Available");
            ViewBag.BorrowedBookCount = _context.Books.Count(b => b.AvailabilityStatus == "Borrowed");
            ViewBag.ArchivedBookCount = _context.Books.Count(b => b.AvailabilityStatus == "Archived");
            ViewBag.OverdueCount = _context.BookBorrowings.Count(bb => bb.Status == "Overdue");

            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => (p.Title != null && p.Title.Contains(searchTerm)) ||
                                         (p.Author != null && p.Author.Contains(searchTerm)) ||
                                         p.AccessionId == searchTerm);
            }

            var books = query.ToList();
            return View("~/Views/Staff/StaffBookManagement.cshtml", books);
        }


        [HttpPost]
        public IActionResult AddBook([FromBody] Book newBook)
        {
            if (newBook == null)
                return Json(new { success = false, message = "Please Insert Book Info." });

            if (string.IsNullOrEmpty(newBook.AccessionId))
                return Json(new { success = false, message = "Accession ID is required." });

            bool exists = _context.Books.Any(b => b.AccessionId == newBook.AccessionId);
            if (exists)
                return Json(new { success = false, message = "Accession ID already exists." });

            if (ModelState.IsValid)
            {
                _context.Books.Add(newBook);
                LogAction($"Added new book: {newBook.Title}", "Book Management");
                _context.SaveChanges();
                return Json(new { success = true, message = "Book added successfully!" });
            }

            return BadRequest(ModelState);
        }
        /*
        [HttpGet]
        public IActionResult EditBook(string id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
                return Json(new { success = false, message = "Book Does not Exist" });
            return View("~/Views/Staff/StaffEditBook.cshtml", book);
        }

        [HttpPost]
        public IActionResult EditBook(Book updatedBook)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Books.Update(updatedBook);
                    LogAction($"Edited book: {updatedBook.AccessionId}", "book");
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
        */
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
            if (updatedBook == null)
                return Json(new { success = false, message = "Book not Found." });

            var existingBook = _context.Books.Find(updatedBook.AccessionId);
            if (existingBook == null)
                return Json(new { success = false, message = "Book Does not exist" });

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
            LogAction($"Updated book via Management UI: {updatedBook.AccessionId}", "book");
            _context.SaveChanges();
            return Json(new { success = true, message = "Book Updated." });
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

                LogAction($"Processed Return for BorrowId: {id}", "book_borrowing");
                _context.SaveChanges();
                return Json(new { success = true, message = "Book Returned." });
            }
            return Json(new { success = false, message = "Errored occurred" });
        }

        [HttpPost]
        public IActionResult ExtendDueDate(int id, DateTime newDate)
        {
            var borrowing = _context.BookBorrowings.Find(id);
            if (borrowing != null)
            {
                borrowing.DueDate = newDate;
                borrowing.Status = "Borrowed";
                LogAction($"Extended due date for BorrowId: {id} to {newDate.ToShortDateString()}", "book_borrowing");
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
                LogAction($"Toggled availability of {id} to {status}", "book");
                _context.SaveChanges();
            }
            return RedirectToAction("BookManagement", "Book");
        }

        public IActionResult PatronBorrow()
        {
            var availableBooks = _context.Books
                .Where(b => b.AvailabilityStatus == "Available")
                .ToList();
            return View("~/Views/Patron/PatronBorrowBooks.cshtml",availableBooks);
        }

        [HttpPost]
        public IActionResult ProcessBorrow(string patronId, string accessionId)
        {
            var book = _context.Books.Find(accessionId);
            var currentuser = HttpContext.Session.GetString("UserId");
            var patronExists = _context.Patrons.Any(p => p.PatronId == patronId);

            if (book == null || book.AvailabilityStatus != "Available")
                return Json(new { success = false, message = "Book is no longer available." });

            if (!patronExists)
                return Json(new { success = false, message = "Invalid Patron ID." });

            if (currentuser != patronId)
                return Json(new { success = false, message = "Invalid Patron ID." });

            var borrowRequest = new BookBorrowing
            {
                PatronId = patronId,
                AccessionId = accessionId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                Status = "Reserved",
                StaffId = null
            };

            book.AvailabilityStatus = "Reserved";

            _context.BookBorrowings.Add(borrowRequest);
            LogAction($"Patron {patronId} requested borrow for {accessionId}", "book_borrowing");
            _context.SaveChanges();

            return Json(new { success = true, message = "Request submitted! Please proceed to the counter." });
        }
        public IActionResult confirmRequest(int id)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.BookBorrowings.Find(id);
            if (request != null)
            {
                request.Status = "Borrowed";
                request.BorrowDate = DateTime.Now;
                request.DueDate = DateTime.Now.AddDays(7);
                LogAction($"Borrow request {request.BorrowId} approved.", "book_borrowing");
                _context.SaveChanges();
            }

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            return Ok(query);
        }

        //accept deny
        public IActionResult denyRequest(int id)
        {
            if (!IsAuthorized("Librarian")) return Unauthorized();
            var request = _context.BookBorrowings.Find(id);
            if (request != null)
            {
                request.Status = "Denied";
                LogAction($"Borrow request {request.BorrowId} denied.", "book_borrowing");
                _context.SaveChanges();
            }

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            return Ok(query);
        }

    }
}