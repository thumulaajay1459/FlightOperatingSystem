namespace BookingService.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Failed,
        Completed
    }

    public enum BookingType
    {
        OneWay,
        RoundTrip,
        MultiCity
    }

    public class Booking
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int FlightId { get; set; }
        public int? ReturnFlightId { get; set; } // For round trip
        public required string BookingReference { get; set; }
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
        public BookingType BookingType { get; set; } = BookingType.OneWay;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Passenger> Passengers { get; set; } = [];
    }
}
