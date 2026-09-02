using System.ComponentModel.DataAnnotations;

namespace FlightService.Models
{
    public class Airline
    {
        [Key]
        public int AirlineId { get; set; }
        
        [Required]
        [StringLength(3, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
