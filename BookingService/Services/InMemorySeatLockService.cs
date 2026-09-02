using BookingService.Data;
using BookingService.Interfaces;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Services
{
    /// <summary>
    /// DB-backed seat lock implementation.
    /// Replace this registration with RedisSeatLockService when Redis is available.
    /// The interface contract stays identical — no other code changes needed.
    /// </summary>
    public class InMemorySeatLockService : ISeatLockService
    {
        private readonly BookingDbContext _context;

        public InMemorySeatLockService(BookingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryLockSeatAsync(
            int flightId, string seatNumber, string userId, TimeSpan duration)
        {
            // Clean up expired locks first
            var expired = await _context.SeatLocks
                .Where(sl => sl.FlightId == flightId
                          && sl.SeatNumber == seatNumber
                          && sl.LockedUntil < DateTime.UtcNow)
                .ToListAsync();

            _context.SeatLocks.RemoveRange(expired);

            // Check if an active lock exists for this seat
            var activeLock = await _context.SeatLocks
                .AnyAsync(sl => sl.FlightId == flightId
                             && sl.SeatNumber == seatNumber
                             && sl.LockedUntil >= DateTime.UtcNow);

            if (activeLock) return false;

            await _context.SeatLocks.AddAsync(new SeatLock
            {
                FlightId = flightId,
                SeatNumber = seatNumber,
                UserId = userId,
                LockedUntil = DateTime.UtcNow.Add(duration)
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSeatLockedAsync(int flightId, string seatNumber) =>
            await _context.SeatLocks
                .AnyAsync(sl => sl.FlightId == flightId
                             && sl.SeatNumber == seatNumber
                             && sl.LockedUntil >= DateTime.UtcNow);

        public async Task ReleaseLockAsync(int flightId, string seatNumber, string userId)
        {
            var locks = await _context.SeatLocks
                .Where(sl => sl.FlightId == flightId
                          && sl.SeatNumber == seatNumber
                          && sl.UserId == userId)
                .ToListAsync();

            _context.SeatLocks.RemoveRange(locks);
            await _context.SaveChangesAsync();
        }
    }
}
