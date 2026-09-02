namespace BookingService.DTOs
{
    public class TicketDto
    {
        public string BookingReference { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }

        // User details
        public string PassengerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Flight details
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string FlightStatus { get; set; } = string.Empty;

        public List<PassengerDto> Passengers { get; set; } = [];
    }
}
