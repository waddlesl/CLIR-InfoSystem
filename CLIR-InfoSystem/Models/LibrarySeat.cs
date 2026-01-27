using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("library_seats")]
    public class LibrarySeat
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("building")]
        public string Building { get; set; }
        [Column("seat_name")]
        public string SeatName { get; set; }
        [Column("seat_type")]
        public string SeatType { get; set; }
    }
}