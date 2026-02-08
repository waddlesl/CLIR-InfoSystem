using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Column("staff_id")]
        public int? StaffId { get; set; }

        [Column("patron_id")]
        public string? PatronId { get; set; }

        [Column("action_performed", TypeName = "TEXT")]
        public string? ActionPerformed { get; set; }

        [Column("table_affected")]
        public string? TableAffected { get; set; }

        [Column("log_date")]
        public DateTime LogDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("StaffId")]
        public virtual Staff? Staff { get; set; }

        [ForeignKey("PatronId")]
        public virtual Patron? Patron { get; set; }
    }
}