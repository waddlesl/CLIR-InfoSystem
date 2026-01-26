using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class TransactionController : Controller
    {
        // 1. Declare the context
        private readonly LibraryDbContext _context;

        // 2. Inject the context through the constructor
        public TransactionController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");

            var myLoans = _context.BookBorrowings
                .Include(b => b.Book) // This line fixes the empty titles
                .Where(b => b.PatronId == userId && b.Status == "Borrowed")
                .ToList();

            return View(myLoans);
        }

        [HttpPost]
        public IActionResult RequestBook(string accessionId)
        {
            var patronId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(patronId))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = new BookBorrowing
            {
                PatronId = patronId,
                AccessionId = accessionId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7), // Sets a 1-week deadline
                Status = "Borrowed"
            };

            _context.BookBorrowings.Add(request);

            // Update book status so others can't request it
            var book = _context.Books.Find(accessionId);
            if (book != null) { book.AvailabilityStatus = "Borrowed"; }

            _context.SaveChanges();

            return RedirectToAction("Index", "Transaction");
        }
    }


}