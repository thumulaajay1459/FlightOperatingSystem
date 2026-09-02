using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class UserProfileDto
    {
        [Required]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        public string Nationality { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
    }

    public class UserProfileResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
    }
}
