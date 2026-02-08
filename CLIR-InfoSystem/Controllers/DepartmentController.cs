using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CLIR_InfoSystem.Controllers
{
    // Inherit from BaseController to use shared _context and LogAction
    public class DepartmentController : BaseController
    {
        public DepartmentController(LibraryDbContext context) : base(context) { }

        // List all departments
        public IActionResult Index()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }

        // API for Modals/Dropdowns
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

            // Log the addition
            LogAction($"Added new department: {dept.DeptName}", "departments");

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteDepartment(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            // Check if any programs are linked
            bool hasLinkedPrograms = _context.Programs.Any(p => p.DeptId == id);
            if (hasLinkedPrograms)
                return Json(new { success = false, message = "Cannot delete department with linked programs." });

            string deptName = dept.DeptName; // Capture name for log
            _context.Departments.Remove(dept);

            // Log the deletion
            LogAction($"Deleted department: {deptName}", "departments");

            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}