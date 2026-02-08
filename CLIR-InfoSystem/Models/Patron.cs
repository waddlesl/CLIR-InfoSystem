using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("patron")]
    public class Patron
    {
        [Key]
        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("patron_type")]
        public string? PatronType { get; set; } // Nullable

        [Column("last_name")]
        public string? LastName { get; set; } // Nullable

        [Column("first_name")]
        public string? FirstName { get; set; } // Nullable

        [Column("middle_name")]
        public string? MiddleName { get; set; } // Nullable

        [Column("dept_id")]
        public int? DeptId { get; set; }

        [Column("program_id")]
        public int? ProgramId { get; set; }

        [Column("year_level")]
        public string? YearLevel { get; set; } // Nullable

        [Column("email")]
        public string? Email { get; set; } // Nullable

        // Navigation
        [ForeignKey("DeptId")]
        public Department? Department { get; set; }

        [ForeignKey("ProgramId")]
        public AcademicProgram? Program { get; set; }
    }
}