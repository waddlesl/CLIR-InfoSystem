using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class ProgramController : Controller
    {
        private readonly LibraryDbContext _context;

        public ProgramController(LibraryDbContext context)
        {
            _context = context;
        }

        // List all programs with their Department names
        public IActionResult Index()
        {
            var programs = _context.Programs.Include(p => p.Department).ToList();
            return View(programs);
        }

        // API for Cascading Dropdowns: Get programs belonging to a specific department
        [HttpGet]
        public IActionResult GetProgramsByDept(int deptId)
        {
            var programs = _context.Programs
                .Where(p => p.DeptId == deptId)
                .Select(p => new { p.ProgramId, p.ProgramName })
                .ToList();

            return Json(programs);
        }

        [HttpPost]
        public IActionResult AddProgram([FromBody] AcademicProgram newProg)
        {
            if (newProg == null) return BadRequest();

            _context.Programs.Add(newProg);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteProgram(int id)
        {
            var program = _context.Programs.Find(id);
            if (program == null) return NotFound();

            // Check if any patrons are currently enrolled in this program
            bool hasPatrons = _context.Patrons.Any(p => p.ProgramId == id);
            if (hasPatrons)
                return Json(new { success = false, message = "Cannot delete program with registered patrons." });

            _context.Programs.Remove(program);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}