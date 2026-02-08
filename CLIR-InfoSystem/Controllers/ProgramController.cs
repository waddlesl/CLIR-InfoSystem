using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    // Inherit from BaseController for shared context and logging
    public class ProgramController : BaseController
    {
        public ProgramController(LibraryDbContext context) : base(context) { }

        // List all programs with Department names
        public IActionResult Index()
        {
            var programs = _context.Programs.Include(p => p.Department).ToList();
            return View(programs);
        }

        // API for Cascading Dropdowns
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

            // Log the addition of a new academic program
            LogAction($"Added academic program: {newProg.ProgramName}", "programs");

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteProgram(int id)
        {
            var program = _context.Programs.Find(id);
            if (program == null) return NotFound();

            // Check for registered patrons before deletion
            bool hasPatrons = _context.Patrons.Any(p => p.ProgramId == id);
            if (hasPatrons)
                return Json(new { success = false, message = "Cannot delete program with registered patrons." });

            string progName = program.ProgramName;
            _context.Programs.Remove(program);

            // Log the deletion
            LogAction($"Deleted academic program: {progName}", "programs");

            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}