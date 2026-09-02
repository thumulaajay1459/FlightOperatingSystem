using System.ComponentModel.DataAnnotations;

namespace FlightService.DTOs
{
    public class AircraftDto
    {
        public int AircraftId { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }

    public class CreateAircraftDto
    {
        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int TotalSeats { get; set; }
    }

    public class UpdateAircraftDto
    {
        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int TotalSeats { get; set; }
    }
}
