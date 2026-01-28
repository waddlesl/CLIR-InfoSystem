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

        [Column("school_year")]
        public int SchoolYear { get; set; } = DateTime.Now.Year;

        [Column("session_date")]
        public DateTime SessionDate { get; set; }

        [Column("topic")]
        public string Topic { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}