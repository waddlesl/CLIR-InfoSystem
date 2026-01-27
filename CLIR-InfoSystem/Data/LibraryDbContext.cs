using Microsoft.EntityFrameworkCore;
using CLIR_InfoSystem.Models;

namespace CLIR_InfoSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookBorrowing> BookBorrowings { get; set; }
        public DbSet<ServiceRequests> ServiceRequests { get; set; }
        public DbSet<SeatBooking> SeatBookings { get; set; }
        public DbSet<LibrarySeat> LibrarySeats { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<LibrarianBooking> LibrarianBookings { get; set; }
        public DbSet<GrammarlyAndTurnitinRequest> GrammarlyAndTurnitinRequests { get; set; }

       
       

    }
}