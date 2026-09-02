using BookingService.DTOs;

namespace BookingService.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingRequestDto request);
        Task<BookingDetailsDto?> GetBookingByIdAsync(int id, string userId);
        Task<List<BookingDetailsDto>> GetBookingsByUserIdAsync(string userId);
        Task<BookingResponseDto> CancelBookingAsync(int id, string userId);
        Task<TicketDto?> GetTicketAsync(int bookingId, string userId);
        Task<List<BookingDetailsDto>> GetAllBookingsAsync();
        Task<List<BookingDetailsDto>> GetBookingsByFlightIdAsync(int flightId);
    }
}
