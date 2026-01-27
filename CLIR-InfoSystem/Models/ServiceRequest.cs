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

        [Column("request_date")]
        public DateTime RequestDate { get; set; }

        [Column("date_of_access_provided")]
        public DateTime? DateAccessProvided { get; set; }

        [Column("staff_in_charge")]
        public string? StaffInCharge { get; set; }

        [Column("date_of_access_removed")]
        public DateTime? DateAccessRemoved { get; set; }

        [Column("request_status")]
        public string RequestStatus { get; set; } = "Pending";

        [Column("service_type")]
        public string ServiceType { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
} // <--- AND THIS CURLY BRACE