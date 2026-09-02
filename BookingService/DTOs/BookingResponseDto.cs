namespace BookingService.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public required string BookingReference { get; set; }
        public required string BookingStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
