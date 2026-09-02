namespace BookingService.DTOs
{
    public class TicketConfirmationDto
    {
        public string RecipientEmail { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string BookingId { get; set; } = null!;
        public string FlightNumber { get; set; } = null!;
        public string DepartureCity { get; set; } = null!;
        public string ArrivalCity { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string SeatNumber { get; set; } = null!;
        public decimal TotalPrice { get; set; }
    }
}
