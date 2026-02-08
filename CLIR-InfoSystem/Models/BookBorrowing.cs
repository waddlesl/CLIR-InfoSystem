using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book_borrowing")]
    public class BookBorrowing
    {
        [Key]
        [Column("borrow_id")]
        public int BorrowId { get; set; }

        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("accession_id")]
        public string AccessionId { get; set; }

        [Column("borrow_date")]
        public DateTime BorrowDate { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        // Navigation
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        [ForeignKey("AccessionId")]
        public Book Book { get; set; }

        [ForeignKey("PatronId")]
        public Patron Patron { get; set; }

        // For views
        [NotMapped]
        public string StaffInCharge =>
            Staff != null ? $"{Staff.FirstName} {Staff.LastName}" : "N/A";
    }
}