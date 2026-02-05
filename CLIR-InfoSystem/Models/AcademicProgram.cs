using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("programs")]
    public class AcademicProgram
    {
        [Key]  // <-- This is required
        [Column("program_id")]
        public int ProgramId { get; set; }          // primary key

        [Column("program_code")]
        public string ProgramCode { get; set; }

        [Column("program_name")]
        public string ProgramName { get; set; }

        [Column("dept_id")]
        public int? DeptId { get; set; }            // foreign key

        // Navigation property
        public Department Department { get; set; }
    }
}
