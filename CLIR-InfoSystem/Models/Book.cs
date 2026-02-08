using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book")]
    public class Book
    {
        [Key]
        [Column("accession_id")]
        public string AccessionId { get; set; } = string.Empty;

        [Column("title")]
        public string? Title { get; set; } // Added ?

        [Column("author")]
        public string? Author { get; set; } // Added ?

        [Column("availability_status")]
        public string? AvailabilityStatus { get; set; } // Added ?

        [Column("edition")]
        public string? Edition { get; set; } // Added ?

        [Column("year_of_publication")]
        public int? YearOfPublication { get; set; } // Added ? to int

        [Column("publisher")]
        public string? Publisher { get; set; } // Added ?

        [Column("collection")]
        public string? Collection { get; set; } // Added ?

        [Column("library_location")]
        public string? LibraryLocation { get; set; } // Added ?

        [Column("supplier")]
        public string? Supplier { get; set; } // Added ?

        [Column("sourced_from")]
        public string? SourcedFrom { get; set; } // Added ?

        [Column("price")]
        public decimal? Price { get; set; } // Added ?

        [Column("discount")]
        public decimal? Discount { get; set; } // Added ?

        [NotMapped]
        public decimal Subtotal => (Price ?? 0) - (Discount ?? 0);
    }
}
