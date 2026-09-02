namespace BookingService.DTOs
{
    public class PassengerDto
    {
        public required string FullName { get; set; }
        public int Age { get; set; }
        public required string Gender { get; set; }
        public string PassengerType { get; set; } = "Adult"; // Adult, Child, Infant
        public required string PassportNumber { get; set; }
        public required string SeatNumber { get; set; }
    }
}
