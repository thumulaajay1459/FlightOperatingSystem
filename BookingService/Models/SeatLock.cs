namespace BookingService.Models
{
    public class SeatLock
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public required string SeatNumber { get; set; }
        public required string UserId { get; set; }
        public DateTime LockedUntil { get; set; }
    }
}
