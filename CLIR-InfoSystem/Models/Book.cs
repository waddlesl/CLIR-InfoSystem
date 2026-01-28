using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book")] 
    public class Book
    {
        [Key]
        [Column("accession_id")] 
        public string AccessionId { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("author")]
        public string? Author { get; set; }

        [Column("availability_status")]
        public string? AvailabilityStatus { get; set; }

        [Column("edition")]
        public string? Edition { get; set; }

        [Column("year_of_publication")]
        public int? YearOfPublication { get; set; }

        [Column("publisher")]
        public string? Publisher { get; set; }

        [Column("collection")]
        public string? Collection { get; set; }
        [Column("library_location")]
        public string? Location { get; set; }

        [Column("supplier")]
        public string? Supplier { get; set; }
        [Column("sourced_from")]
        public string? Source { get; set; }

        [Column("price")]
        public int? Price { get; set; }
        [Column("discount")]
        public int? Discount { get; set; }

        [Column("subtotal")]
        public int? Subtotal { get; set; }
    }
}