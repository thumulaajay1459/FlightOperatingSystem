using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    public class UpdateBookingDto
    {
        public List<PassengerDto> Passengers { get; set; } = [];
    }
}
