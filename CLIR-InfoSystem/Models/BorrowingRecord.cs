using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book_borrowing")]
    public class BookBorrowing
    {
        //add "?" to make it nullable hehe
        [Key]
        [Column("borrow_id")]
        public int BorrowId { get; set; }

        [Column("patron_id")]
        public string? PatronId { get; set; }

        [Column("accession_id")]
        public string? AccessionId { get; set; }

        [Column("borrow_date")]
        public DateTime BorrowDate { get; set; }

        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        [Column("status")]
        public string Status { get; set; } // 'Borrowed', 'Returned', 'Overdue'

        [Column("staff_in_charge")]
        public string? StaffInCharge { get; set; }

        //foreign keys

        [ForeignKey("AccessionId")]
        public virtual Book? Book { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}