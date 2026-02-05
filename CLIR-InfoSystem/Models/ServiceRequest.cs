using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("services")]
    public class ServiceRequest
    {
        [Key]
        [Column("service_id")]
        public int ServiceId { get; set; }

        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        [Column("request_date")]
        public DateTime RequestDate { get; set; }

        [Column("service_type")]
        public string ServiceType { get; set; }

        [Column("request_status")]
        public string RequestStatus { get; set; } = "Pending";

        // Navigation
        [ForeignKey("PatronId")]
        public Patron Patron { get; set; }

        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }
    }
}
