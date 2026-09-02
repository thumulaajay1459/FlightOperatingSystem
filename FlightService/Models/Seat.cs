using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightService.Models
{
    public enum SeatClass
    {
        Economy,
        PremiumEconomy,
        Business,
        FirstClass
    }

    public enum SeatStatus
    {
        Available,
        Selected,
        Booked,
        Blocked,
        Unavailable
    }

    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        [Required]
        public int AircraftId { get; set; }

        [Required]
        public string SeatNumber { get; set; } = string.Empty; // e.g., "12A", "15F"

        [Required]
        public int Row { get; set; }

        [Required]
        public string Column { get; set; } = string.Empty; // A, B, C, D, E, F

        [Required]
        public SeatClass Class { get; set; } = SeatClass.Economy;

        public bool IsWindowSeat { get; set; }
        public bool IsAisleSeat { get; set; }
        public bool IsExitRow { get; set; }
        public bool HasExtraLegroom { get; set; }

        public decimal ExtraCharge { get; set; } = 0; // Extra charge for premium seats

        [ForeignKey("AircraftId")]
        public Aircraft? Aircraft { get; set; }
    }

    public class FlightSeat
    {
        [Key]
        public int FlightSeatId { get; set; }

        [Required]
        public int FlightId { get; set; }

        [Required]
        public int SeatId { get; set; }

        [Required]
        public SeatStatus Status { get; set; } = SeatStatus.Available;

        public int? BookingId { get; set; } // Null if not booked

        public string? BlockedByUserId { get; set; } // Temporary block during booking process

        public DateTime? BlockedUntil { get; set; } // Block expiry time

        [ForeignKey("FlightId")]
        public Flight? Flight { get; set; }

        [ForeignKey("SeatId")]
        public Seat? Seat { get; set; }
    }
}
