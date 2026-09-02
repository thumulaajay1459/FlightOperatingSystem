namespace BookingService.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum PassengerType
    {
        Adult,      // 12+ years
        Child,      // 2-11 years
        Infant      // 0-23 months
    }

    public class Passenger
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public required string FullName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public PassengerType PassengerType { get; set; } = PassengerType.Adult;
        public required string PassportNumber { get; set; }
        public required string SeatNumber { get; set; }
        public decimal TicketPrice { get; set; } = 0; // Individual ticket price
    }
}
