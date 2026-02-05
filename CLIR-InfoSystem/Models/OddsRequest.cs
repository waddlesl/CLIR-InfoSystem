using System;
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
        public Patron Patron { get; set; }

        [Column("type_of_material")]
        public string MaterialType { get; set; }      // matches controller/view

        [Column("type_of_service")]
        public string ServiceType { get; set; }       // matches controller/view

        [Column("accession_id")]
        public string AccessionId { get; set; }
        public Book Book { get; set; }

        [Column("link")]
        public string ResourceLink { get; set; }      // matches controller/view

        [Column("request_date")]
        public DateTime RequestDate { get; set; }     // was missing

        [Column("date_needed")]
        public DateTime? DateNeeded { get; set; }

        [Column("request_status")]
        public string RequestStatus { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }
        public Staff Staff { get; set; }

        [Column("due_date_of_viewing")]
        public DateTime? DueDateOfViewing { get; set; }

        [Column("date_of_access_provided")]
        public DateTime? DateOfAccessProvided { get; set; }
    }
}
