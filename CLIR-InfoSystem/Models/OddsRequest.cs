using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("odds")]
    public class OddsRequest
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("request_date")]
        public DateTime? RequestDate { get; set; }

        [Column("type_of_material")]
        public string MaterialType { get; set; }

        [Column("type_of_service")]
        public string ServiceType { get; set; }

        [Column("accession_id")]
        public string? AccessionId { get; set; }

        [Column("date_needed")]
        public DateTime? DateNeeded { get; set; }

        [Column("request_status")]
        public string? RequestStatus { get; set; } = "Pending";

        [Column("staff_in_charge")]
        public string? StaffInCharge { get; set; }

        [Column("due_date_of_viewing")]
        public DateTime? DueDateofViewing { get; set; }

        [Column("date_of_access_provided")]
        public DateTime? DateAccessProvided { get; set; }

        [Column("link")]
        public string? ResourceLink { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }

        [ForeignKey("AccessionId")]
        public virtual Book? Book { get; set; }
    }
}