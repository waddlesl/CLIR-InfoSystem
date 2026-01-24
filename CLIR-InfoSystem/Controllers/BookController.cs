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

        public IActionResult BookManagement()
        {
            ViewBag.BookCount = _context.Books.Count();
            ViewBag.BorrowedBookCount = _context.BookBorrowings.Count();
            ViewBag.OverdueBookCount = _context.BookBorrowings.Count(b=>b.Status == "Overdue");


            var books = _context.Books.ToList();
            ViewBag.BorrowedBooks = _context.BookBorrowings.Include(bb => bb.Book).Include(bb => bb.Patron).ToList();
            return View(books); 
        
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddBook(Book newBook)
        {
            if (ModelState.IsValid)
            {
                
                _context.Books.Add(newBook);
                _context.SaveChanges();

                return RedirectToAction("BookManagement", "Book");
            }
           
            return View(newBook);
        }

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
    }
}
