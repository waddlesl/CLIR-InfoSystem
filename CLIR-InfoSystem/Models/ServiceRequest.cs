using System.ComponentModel.DataAnnotations;
namespace CLIR_InfoSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }
        public int PatronId { get; set; }
        public string ServiceType { get; set; } // "Grammarly", "Turnitin", or "ODDS"
        public DateTime RequestDate { get; set; }
        public string RequestStatus { get; set; }
        public string StaffInCharge { get; set; }

        // ODDS Specific
        public string? MaterialType { get; set; }
        public string? ResourceLink { get; set; }
    }
}