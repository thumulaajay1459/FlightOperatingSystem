using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Nationality { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
