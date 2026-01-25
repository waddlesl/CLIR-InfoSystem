using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CLIR_InfoSystem.Models
{
    [Table("odds")]
    public class ServiceRequest
    {

        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }
        [Column("patron_id")]
        public string PatronId { get; set; }
        [Column("type_of_material")]
        public string MaterialType { get; set; } // 'E-Book', 'Journal Article', 'Thesis', 'Conference Paper'
        [Column("type_of_service")]
        public string ServiceType { get; set; } // 'Scanning', 'PDF Delivery', 'Resource Link'
        [Column("request_date")]
        public DateTime RequestDate { get; set; }
        [Column("request_status")]
        public string RequestStatus { get; set; } //'denied', 'Fulfilled', 'Cancelled'
        [Column("staff_in_charge")]
        public string StaffInCharge { get; set; }
        [Column("date_of_access_provided")]
        public DateTime DateAccessProvided { get; set; }
        public string? ResourceLink { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}