using System.ComponentModel.DataAnnotations;

namespace CLIR_InfoSystem.Models
{
    public class SeatBooking
    {
        [Key]
        public int SeatId { get; set; }
        public int PatronId { get; set; }
        public string Term { get; set; }
        public DateTime Time { get; set; }
        public string PreferredSeating { get; set; }
        public int GroupSize { get; set; }
        public string Building { get; set; }
    }
}
