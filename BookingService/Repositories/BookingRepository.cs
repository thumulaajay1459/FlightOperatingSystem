using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetByIdAsync(int id) =>
            await _context.Bookings
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<Booking?> GetByReferenceAsync(string bookingReference) =>
            await _context.Bookings
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);

        public async Task<List<Booking>> GetByUserIdAsync(string userId) =>
            await _context.Bookings
                .Include(b => b.Passengers)
                .Where(b => b.UserId == userId)
                .ToListAsync();

        public async Task<List<Booking>> GetAllAsync() =>
            await _context.Bookings
                .Include(b => b.Passengers)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<List<Booking>> GetByFlightIdAsync(int flightId) =>
            await _context.Bookings
                .Include(b => b.Passengers)
                .Where(b => b.FlightId == flightId)
                .ToListAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
