using System.ComponentModel.DataAnnotations;

namespace BookingService.DTOs
{
    public class EnhancedPassengerDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Range(0, 120)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty; // Male, Female, Other

        [Required]
        public string PassengerType { get; set; } = "Adult"; // Adult, Child, Infant

        [Required]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        public string? SeatClass { get; set; } = "Economy"; // Economy, PremiumEconomy, Business, FirstClass
    }

    public class RoundTripBookingRequestDto
    {
        [Required]
        public int OutboundFlightId { get; set; }

        public int? ReturnFlightId { get; set; } // Null for one-way

        [Required]
        public string BookingType { get; set; } = "OneWay"; // OneWay, RoundTrip

        [Required]
        public List<EnhancedPassengerDto> Passengers { get; set; } = [];

        // Pricing breakdown
        public decimal BaseFare { get; set; }
        public decimal Taxes { get; set; }
        public decimal SeatCharges { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SeatSelectionDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }

    public class SeatBlockRequestDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public List<string> SeatNumbers { get; set; } = [];

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int BlockDurationMinutes { get; set; } = 10; // Default 10 minutes
    }

    public class SeatMapDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public List<SeatDto> Seats { get; set; } = [];
        public SeatLayoutDto Layout { get; set; } = new();
    }

    public class SeatDto
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int Row { get; set; }
        public string Column { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Available, Selected, Booked, Blocked
        public bool IsWindowSeat { get; set; }
        public bool IsAisleSeat { get; set; }
        public bool IsExitRow { get; set; }
        public bool HasExtraLegroom { get; set; }
        public decimal ExtraCharge { get; set; }
        public DateTime? BlockedUntil { get; set; }
    }

    public class SeatLayoutDto
    {
        public int TotalRows { get; set; }
        public List<string> Columns { get; set; } = []; // ["A", "B", "C", "D", "E", "F"]
        public Dictionary<string, int> ClassRowRanges { get; set; } = new(); // {"Economy": "1-20", "Business": "21-25"}
    }

    public class PriceCalculationRequestDto
    {
        [Required]
        public int FlightId { get; set; }

        public int? ReturnFlightId { get; set; }

        [Required]
        public List<PassengerPricingDto> Passengers { get; set; } = [];
    }

    public class PassengerPricingDto
    {
        public string PassengerType { get; set; } = "Adult"; // Adult, Child, Infant
        public string SeatClass { get; set; } = "Economy";
        public string? SeatNumber { get; set; }
    }

    public class PriceBreakdownDto
    {
        public decimal BaseFare { get; set; }
        public decimal ChildDiscount { get; set; }
        public decimal InfantDiscount { get; set; }
        public decimal SeatCharges { get; set; }
        public decimal Taxes { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PassengerPriceDto> PassengerPrices { get; set; } = [];
    }

    public class PassengerPriceDto
    {
        public string PassengerType { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal SeatCharge { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
