using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLIR_InfoSystem.Models
{
    [Table("staff")]
    public class Staff
    {
        [Key]
        [Column("staff_id")]
        public int StaffId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("type_of_user")]
        public string TypeOfUser { get; set; } // Admin, Librarian, Student Assistant

        [Column("status")]
        public string Status { get; set; } // Active, Inactive
    }
}