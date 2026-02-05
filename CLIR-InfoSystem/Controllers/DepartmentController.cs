using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly LibraryDbContext _context;

        public DepartmentController(LibraryDbContext context)
        {
            _context = context;
        }

        // List all departments
        public IActionResult Index()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }

        // API for Modals/Dropdowns (returns JSON for the Patron Registration)
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var depts = _context.Departments
                .Select(d => new { d.DeptId, d.DeptName })
                .ToList();
            return Json(depts);
        }

        [HttpPost]
        public IActionResult AddDepartment([FromBody] Department dept)
        {
            if (dept == null) return BadRequest();

            _context.Departments.Add(dept);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteDepartment(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            // Check if any programs are linked to this department before deleting
            bool hasLinkedPrograms = _context.Programs.Any(p => p.DeptId == id);
            if (hasLinkedPrograms)
                return Json(new { success = false, message = "Cannot delete department with linked programs." });

            _context.Departments.Remove(dept);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}