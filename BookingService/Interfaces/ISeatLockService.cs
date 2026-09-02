namespace BookingService.Interfaces
{
    /// <summary>
    /// Abstraction for seat locking.
    /// Current implementation: in-memory (DB-backed).
    /// Future implementation: Redis distributed lock — swap by changing DI registration only.
    /// </summary>
    public interface ISeatLockService
    {
        /// <summary>Attempts to lock a seat for a user. Returns false if already locked.</summary>
        Task<bool> TryLockSeatAsync(int flightId, string seatNumber, string userId, TimeSpan duration);

        /// <summary>Checks whether a seat is currently locked by any user.</summary>
        Task<bool> IsSeatLockedAsync(int flightId, string seatNumber);

        /// <summary>Releases a seat lock held by the specified user.</summary>
        Task ReleaseLockAsync(int flightId, string seatNumber, string userId);
    }
}
