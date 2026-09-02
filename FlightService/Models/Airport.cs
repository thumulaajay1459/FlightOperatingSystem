using System.ComponentModel.DataAnnotations;

namespace FlightService.Models
{
    public class Airport
    {
        [Key]
        public int AirportId { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;
    }
}
