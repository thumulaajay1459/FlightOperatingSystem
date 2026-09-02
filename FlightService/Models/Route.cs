using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightService.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required]
        public int OriginAirportId { get; set; }

        [Required]
        public int DestinationAirportId { get; set; }

        public int DistanceKm { get; set; }

        [ForeignKey("OriginAirportId")]
        public Airport? Origin { get; set; }

        [ForeignKey("DestinationAirportId")]
        public Airport? Destination { get; set; }
    }
}
