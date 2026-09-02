namespace BookingService.DTOs
{
    public class BookingDetailsDto
    {
        public int Id { get; set; }
        public required string BookingReference { get; set; }
        public required string UserId { get; set; }
        public int FlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public string BookingType { get; set; } = "OneWay";
        public required string BookingStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PassengerDto> Passengers { get; set; } = [];
        
        // Flight details
        public string? FlightNumber { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string? FlightStatus { get; set; }
    }
}
