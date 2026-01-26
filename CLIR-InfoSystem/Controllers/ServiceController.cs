using CLIR_InfoSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLIR_InfoSystem.Controllers
{
    public class ServiceController : Controller
    {
        private readonly LibraryDbContext _context;

        public ServiceController(LibraryDbContext context)
        {
            _context = context;
        }

        public IActionResult ManageODDS()
        {
            var odds = _context.ServiceRequests
                .Include(s => s.Patron)
                .OrderByDescending(o => o.RequestDate)
                .ToList();

            return View(odds);
        }

        public IActionResult Index()
        {
            return View();
        }
    }

    }

