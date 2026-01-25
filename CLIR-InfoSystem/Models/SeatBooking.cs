using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book_a_seat")]
    public class SeatBooking
    {
        [Key]
        [Column("seat_id")]
        public int SeatId { get; set; }
        [Column("patron_id")]
        public string PatronId { get; set; }
        [Column("term")]
        public string Term { get; set; }
        [Column("time")]
        public DateTime Time { get; set; }
        [Column("preferred_seating")]
        public string PreferredSeating { get; set; }
        [Column("group_size")]
        public int GroupSize { get; set; }
        [Column("building")]
        public string Building { get; set; }


        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}
