using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book_a_librarian")]
    public class BookALibrarian
    {
        [Key]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("patron_id")]
        public string PatronId { get; set; }
        public Patron Patron { get; set; }

        [Column("staff_id")]
        public int StaffId { get; set; }
        public Staff Staff { get; set; }

        [Column("school_year")]
        public string SchoolYear { get; set; }

        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Column("topic")]
        public string Topic { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}
