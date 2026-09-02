using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetByReferenceAsync(string bookingReference);
        Task<List<Booking>> GetByUserIdAsync(string userId);
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetByFlightIdAsync(int flightId);
        Task SaveChangesAsync();
    }
}
