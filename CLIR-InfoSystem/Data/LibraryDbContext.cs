using Microsoft.EntityFrameworkCore;
using CLIR_InfoSystem.Models;

namespace CLIR_InfoSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        // Core Tables
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AcademicProgram> Programs { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Book> Books { get; set; }

        // Activity Tables
        public DbSet<BookBorrowing> BookBorrowings { get; set; }
        public DbSet<ServiceRequest> Services { get; set; }
        public DbSet<OddsRequest> Odds { get; set; }
        public DbSet<SeatBooking> SeatBookings { get; set; }
        public DbSet<LibrarySeat> LibrarySeats { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<BookALibrarian> BookALibrarians { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Program -> Department
            modelBuilder.Entity<AcademicProgram>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Programs)
                .HasForeignKey(p => p.DeptId)
                .OnDelete(DeleteBehavior.SetNull);

            // Patron -> Department & Program
            modelBuilder.Entity<Patron>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Patrons)
                .HasForeignKey(p => p.DeptId);

            modelBuilder.Entity<Patron>()
                .HasOne(p => p.Program)
                .WithMany()
                .HasForeignKey(p => p.ProgramId);

            // BookALibrarian
            modelBuilder.Entity<BookALibrarian>()
                .HasOne(b => b.Patron)
                .WithMany()
                .HasForeignKey(b => b.PatronId);

            modelBuilder.Entity<BookALibrarian>()
                .HasOne(b => b.Staff)
                .WithMany()
                .HasForeignKey(b => b.StaffId);

            // SeatBooking
            modelBuilder.Entity<SeatBooking>()
                .HasOne(s => s.LibrarySeat)
                .WithMany()
                .HasForeignKey(s => s.SeatId);

            modelBuilder.Entity<SeatBooking>()
                .HasOne(s => s.TimeSlot)
                .WithMany()
                .HasForeignKey(s => s.SlotId);

            modelBuilder.Entity<SeatBooking>()
                .HasOne(s => s.Patron)
                .WithMany()
                .HasForeignKey(s => s.PatronId);

            // BookBorrowing
            modelBuilder.Entity<BookBorrowing>()
                .HasOne(bb => bb.Book)
                .WithMany()
                .HasForeignKey(bb => bb.AccessionId);

            modelBuilder.Entity<BookBorrowing>()
                .HasOne(bb => bb.Patron)
                .WithMany()
                .HasForeignKey(bb => bb.PatronId);

            modelBuilder.Entity<BookBorrowing>()
                .HasOne(bb => bb.Staff)
                .WithMany()
                .HasForeignKey(bb => bb.StaffId);

            // ServiceRequest
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(s => s.Patron)
                .WithMany()
                .HasForeignKey(s => s.PatronId);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(s => s.Staff)
                .WithMany()
                .HasForeignKey(s => s.StaffId);

            // OddsRequest
            modelBuilder.Entity<OddsRequest>()
                .HasOne(o => o.Patron)
                .WithMany()
                .HasForeignKey(o => o.PatronId);

            modelBuilder.Entity<OddsRequest>()
                .HasOne(o => o.Book)
                .WithMany()
                .HasForeignKey(o => o.AccessionId);

            modelBuilder.Entity<OddsRequest>()
                .HasOne(o => o.Staff)
                .WithMany()
                .HasForeignKey(o => o.StaffId);
        }
    }
}
