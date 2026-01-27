using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("book_a_seat")]
    public class SeatBooking
    {
        [Key]
        [Column("booking_id")]
        public int BookingId { get; set; }

        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("seat_id")]
        public int SeatId { get; set; }

        [Column("slot_id")]
        public int SlotId { get; set; }

        [Column("booking_date")]
        public DateTime BookingDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "Reserved";

        // Navigation Properties (Required for ReportController)
        [ForeignKey("SeatId")]
        public virtual LibrarySeat? LibrarySeat { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}