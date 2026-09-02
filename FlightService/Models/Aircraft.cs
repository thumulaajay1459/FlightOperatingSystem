using System.ComponentModel.DataAnnotations;

namespace FlightService.Models
{
    public class Aircraft
    {
        [Key]
        public int AircraftId { get; set; }

        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        public int TotalSeats { get; set; }

        public ICollection<Flight>? Flights { get; set; }
    }
}
