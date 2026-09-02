using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Passenger> Passengers => Set<Passenger>();
        public DbSet<SeatLock> SeatLocks => Set<SeatLock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Booking: unique reference per booking
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Booking → Passengers (cascade delete passengers when booking is deleted)
            modelBuilder.Entity<Passenger>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Passengers)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // SeatLock: composite index on FlightId + SeatNumber for fast lookup
            modelBuilder.Entity<SeatLock>()
                .HasIndex(sl => new { sl.FlightId, sl.SeatNumber });
        }
    }
}
