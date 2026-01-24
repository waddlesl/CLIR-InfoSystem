using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("patron")] // Matches your SQL: CREATE TABLE patron
    public class Patron
    {
        [Key]
        [Column("patron_id")]
        public string PatronId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("department")]
        public string Department { get; set; }

        [Column("program")]
        public string Program { get; set; }
    }
}