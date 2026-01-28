using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class PatronController : Controller
{
    private readonly LibraryDbContext _context;
    public PatronController(LibraryDbContext context) => _context = context;

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        // Fetch all related activities
        ViewBag.SeatHistory = _context.SeatBookings
            .Include(s => s.TimeSlot)
            .Where(s => s.PatronId == userId)
            .OrderByDescending(s => s.BookingDate).ToList();

        ViewBag.OddsHistory = _context.Odds
            .Where(o => o.PatronId == userId)
            .OrderByDescending(o => o.RequestDate).ToList();

        ViewBag.ConsultationHistory = _context.BookALibrarians
            .Where(c => c.PatronId == userId)
            .OrderByDescending(c => c.SessionDate).ToList();

        return View();
    }

    public IActionResult ManagePatrons(string searchTerm)
    {
        var query = _context.Patrons.AsQueryable();
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.FirstName.Contains(searchTerm) ||
                                     p.LastName.Contains(searchTerm) ||
                                     p.PatronId == searchTerm);
        }
        return View(query.ToList());
    }

    [HttpPost]
    public IActionResult AddPatron([FromBody] Patron p)
    {
        // 1. Check for Duplicate ID
        if (_context.Patrons.Any(x => x.PatronId == p.PatronId))
            return Json(new { success = false, message = "ID already exists" });

        // 2. Validate against SQL Constraint pairs
        if (!IsValidDeptProg(p.Department, p.Program))
            return Json(new { success = false, message = "Invalid Department and Program combination." });

        try
        {
            _context.Patrons.Add(p);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetPatronDetails(string id) => Json(_context.Patrons.Find(id));

    [HttpPost]
    public IActionResult UpdatePatron([FromBody] Patron updatedPatron)
    {
        var patron = _context.Patrons.Find(updatedPatron.PatronId);
        if (patron == null)
            return Json(new { success = false, message = "Patron not found" });

        // Validate against SQL Constraint pairs
        if (!IsValidDeptProg(updatedPatron.Department, updatedPatron.Program))
            return Json(new { success = false, message = "Invalid Department and Program combination." });

        patron.FirstName = updatedPatron.FirstName;
        patron.LastName = updatedPatron.LastName;
        patron.Email = updatedPatron.Email;
        patron.Department = updatedPatron.Department;
        patron.Program = updatedPatron.Program;

        try
        {
            _context.SaveChanges();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    public IActionResult PatronDashboard()
    {
        // 1. Get the ID
        var userIdString = HttpContext.Session.GetString("UserId");

        // 2. FETCH the patron from DB 
        // Make sure your Patron ID in the DB is also a string/GUID if that's what's in the session
        var patron = _context.Patrons.FirstOrDefault(p => p.PatronId == userIdString);

        // 3. SET the name (Fallback to "Patron" if null)
        ViewBag.PatronName = patron?.FirstName ?? "Dear Patron";

        // 4. Fill your other counts...
        return View();
    }

    // HELPER: Matches your SQL CHECK CONSTRAINT exactly
    private bool IsValidDeptProg(string dept, string prog)
    {
        return dept switch
        {
            "CAS" => new[] { "BACOMM", "BMMA" }.Contains(prog),
            "CCIS" => new[] { "BSCS", "BSIT" }.Contains(prog),
            "CHS" => new[] { "BSBIO", "BSMEDTECH", "BSPHAR", "BSPT", "BSPSY" }.Contains(prog),
            "CN" => prog == "BSN",
            "ETYCB" => new[] { "BSA", "BSAIS", "BSBA", "BSHM", "BSTM", "BSGM", "BSBIA" }.Contains(prog),
            "MITL" => new[] { "BSARC", "BSCE", "BSCHE", "BSCPE", "BSEE", "BSECE", "BSIE", "BSME" }.Contains(prog),
            "MIA" => new[] { "BSAE", "BSAVM" }.Contains(prog),
            "CMET" => new[] { "BSMARE", "BSMT" }.Contains(prog),
            "SHS" => new[] { "STEM", "ABM", "HUMSS", "ICT" }.Contains(prog),
            _ => false
        };
    }
}