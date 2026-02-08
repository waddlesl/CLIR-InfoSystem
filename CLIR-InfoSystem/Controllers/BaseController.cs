using CLIR_InfoSystem.Data;
using CLIR_InfoSystem.Models;
using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    protected readonly LibraryDbContext _context;

    public BaseController(LibraryDbContext context)
    {
        _context = context;
    }

    // This is the "Universal Helper"
    protected void LogAction(string action, string table)
    {
        // Try to get both. One will be null depending on who is logged in.
        int? staffId = HttpContext.Session.GetInt32("StaffId");
        string? patronId = HttpContext.Session.GetString("PatronId");

        var log = new AuditLog
        {
            StaffId = staffId,
            PatronId = patronId,
            ActionPerformed = action,
            TableAffected = table,
            LogDate = DateTime.Now
        };

        _context.AuditLogs.Add(log);
        // Note: No SaveChanges here if you are calling it in the main method
    }
}