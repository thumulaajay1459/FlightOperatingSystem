namespace BookingService.DTOs
{
    public class CreateBookingRequestDto
    {
        public int FlightId { get; set; }
        public int? ReturnFlightId { get; set; } // For round trip bookings
        public string BookingType { get; set; } = "OneWay"; // OneWay, RoundTrip, MultiCity
        public decimal TotalAmount { get; set; }

        // If true, passenger details are auto-filled from the user's saved profile
        public bool UseProfileAsPassenger { get; set; } = false;

        // SeatNumber is required when UseProfileAsPassenger = true
        public string? SeatNumber { get; set; }

        // Required when UseProfileAsPassenger = false
        public List<PassengerDto> Passengers { get; set; } = [];
    }
}
