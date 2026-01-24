using Microsoft.EntityFrameworkCore;
using CLIR_InfoSystem.Models;

namespace CLIR_InfoSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<User> Staff { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookBorrowing> BookBorrowings { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<SeatBooking> SeatBookings { get; set; }


    }
}