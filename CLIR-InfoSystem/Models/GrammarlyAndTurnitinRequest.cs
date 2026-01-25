using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CLIR_InfoSystem.Models
{
    [Table("services")]
    public class GrammarlyAndTurnitinRequest
    {

        [Key]
        [Column("service_id")]
        public int ServiceId { get; set; }
        [Column("patron_id")]
        public string PatronId { get; set; }
        [Column("service_type")]
        public string ServiceType { get; set; } // "Grammarly" or "Turnitin"
        [Column("request_date")]
        public DateTime RequestDate { get; set; }
        [Column("request_status")]
        public string RequestStatus { get; set; }
        [Column("staff_in_charge")]
        public string StaffInCharge { get; set; }

        // ODDS Specific
        public string? MaterialType { get; set; }
        public string? ResourceLink { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}