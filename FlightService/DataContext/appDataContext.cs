using Microsoft.EntityFrameworkCore;
using FlightService.Models;
using FlightRoute = FlightService.Models.Route;

namespace FlightService.Data
{
    public class appDataContext : DbContext
    {
        public appDataContext(DbContextOptions<appDataContext> options) : base(options)
        {
            
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<FlightRoute> Routes { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<FlightSeat> FlightSeats { get; set; }
        public DbSet<Airline> Airlines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FlightRoute>()
                .HasOne(r => r.Origin)
                .WithMany()
                .HasForeignKey(r => r.OriginAirportId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FlightRoute>()
                .HasOne(r => r.Destination)
                .WithMany()
                .HasForeignKey(r => r.DestinationAirportId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Airport>().HasData(
                new Airport { AirportId = 1, Code = "DEL", Name = "Indira Gandhi International Airport", City = "Delhi", Country = "India" },
                new Airport { AirportId = 2, Code = "BOM", Name = "Chhatrapati Shivaji Maharaj International Airport", City = "Mumbai", Country = "India" },
                new Airport { AirportId = 3, Code = "BLR", Name = "Kempegowda International Airport", City = "Bangalore", Country = "India" },
                new Airport { AirportId = 4, Code = "MAA", Name = "Chennai International Airport", City = "Chennai", Country = "India" },
                new Airport { AirportId = 5, Code = "HYD", Name = "Rajiv Gandhi International Airport", City = "Hyderabad", Country = "India" }
            );
            
            modelBuilder.Entity<Airline>().HasData(
                new Airline { AirlineId = 1, Code = "AI", Name = "Air India", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 2, Code = "6E", Name = "IndiGo", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 3, Code = "UK", Name = "Vistara", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 4, Code = "SG", Name = "SpiceJet", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 5, Code = "G8", Name = "Go First", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 6, Code = "I5", Name = "AirAsia India", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Airline { AirlineId = 7, Code = "QP", Name = "Akasa Air", Country = "India", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }
    }
}
