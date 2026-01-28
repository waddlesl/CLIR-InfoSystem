using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count();
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

        /* MOVED
        public IActionResult BookBorrowers(string searchTerm)
        {
            
            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count();
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
                                         p.BorrowId.ToString() == searchTerm); 
            }

         
            var results = query.ToList();
            return View(results);
        }

        */

        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

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

        /*
        [HttpGet]
        public IActionResult AddBorrower()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddBorrower(BookBorrowing newBorrower)
        {
            newBorrower.BorrowDate = DateTime.Now;
            if (ModelState.IsValid)
            {

                _context.BookBorrowings.Add(newBorrower);
                _context.SaveChanges();

                return RedirectToAction("BookManagement", "Book");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
            }
            return View(newBorrower);
        }
        */

        [HttpGet]
        public IActionResult EditBook(string id)
        {
            var book = _context.Books.Find(id);

            if (book != null)
            {
                return View(book);
            }

            return NotFound();
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
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

            existingBook.Title = updatedBook.Title;
            existingBook.Author = updatedBook.Author;
            existingBook.AvailabilityStatus = updatedBook.AvailabilityStatus;
            existingBook.Edition = updatedBook.Edition;
            existingBook.YearOfPublication = updatedBook.YearOfPublication;
            existingBook.Publisher = updatedBook.Publisher;
            existingBook.Collection = updatedBook.Collection;
            existingBook.Location = updatedBook.Location;
            existingBook.Supplier = updatedBook.Supplier;
            existingBook.Source = updatedBook.Source;
            existingBook.Subtotal = updatedBook.Subtotal;
            existingBook.Price = updatedBook.Price;
            existingBook.Discount = updatedBook.Discount;
            existingBook.Subtotal = updatedBook.Subtotal;
            

            _context.SaveChanges();
            return Ok();
        }

        // Accept Borrow Request
        public IActionResult confirmRequest(int id)
        {
            var request = _context.BookBorrowings.Find(id);
            if (request != null)
            {
                request.Status = "Borrowed";
                request.BorrowDate = DateTime.Now;
                request.DueDate = DateTime.Now.AddDays(7);
                _context.SaveChanges();
            }

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            return Ok(query);
        }

        //deny
        public IActionResult denyRequest(int id)
        {
            var request = _context.BookBorrowings.Find(id);
            if (request != null)
            {
                request.Status = "Denied";
                _context.SaveChanges();
            }

            var query = _context.BookBorrowings
                .Include(bb => bb.Book)
                .Include(bb => bb.Patron)
                .AsQueryable();

            return Ok(query);
        }

        [HttpGet]
        public IActionResult ToggleAvailability(string id, string status)
        {
            var book = _context.Books.FirstOrDefault(b => b.AccessionId == id);
            if (book != null)
            {
                book.AvailabilityStatus = status;
                _context.SaveChanges();
            }
            return RedirectToAction("BookManagement");
        }
        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var borrowing = _context.BookBorrowings.Include(b => b.Book).FirstOrDefault(b => b.BorrowId == id);
            if (borrowing != null)
            {
                borrowing.Status = "Returned";
                borrowing.ReturnDate = DateTime.Now;

                // Make the book available again in the Books table
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
    }
}
