using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("time_slots")]
    public class TimeSlot
    {
        [Key]
        [Column("slot_id")]
        public int SlotId { get; set; }

        [Column("display_text")]
        public string DisplayText { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }
    }
}
