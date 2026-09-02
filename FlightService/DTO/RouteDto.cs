using System.ComponentModel.DataAnnotations;

namespace FlightService.DTOs
{
    public class RouteDto
    {
        public int RouteId { get; set; }
        public int OriginAirportId { get; set; }
        public int DestinationAirportId { get; set; }
        public int DistanceKm { get; set; }
    }

    public class CreateRouteDto
    {
        [Required]
        public int OriginAirportId { get; set; }

        [Required]
        public int DestinationAirportId { get; set; }

        [Required]
        public int DistanceKm { get; set; }
    }

    public class UpdateRouteDto
    {
        [Required]
        public int OriginAirportId { get; set; }

        [Required]
        public int DestinationAirportId { get; set; }

        [Required]
        public int DistanceKm { get; set; }
    }
}
