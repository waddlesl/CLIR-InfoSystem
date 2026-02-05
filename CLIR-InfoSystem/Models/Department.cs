using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("departments")]
    public class Department
    {
        [Key]
        [Column("dept_id")]
        public int DeptId { get; set; }

        [Column("dept_code")]
        public string DeptCode { get; set; }

        [Column("dept_name")]
        public string DeptName { get; set; }

        // Navigation
        public ICollection<AcademicProgram> Programs { get; set; }
        public ICollection<Patron> Patrons { get; set; }
    }
}
