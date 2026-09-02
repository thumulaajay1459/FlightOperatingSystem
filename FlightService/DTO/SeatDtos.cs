using System.ComponentModel.DataAnnotations;

namespace FlightService.DTOs
{
    public class CreateSeatDto
    {
        [Required]
        public int AircraftId { get; set; }

        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public int Row { get; set; }

        [Required]
        public string Column { get; set; } = string.Empty;

        [Required]
        public string Class { get; set; } = "Economy";

        public bool IsWindowSeat { get; set; }
        public bool IsAisleSeat { get; set; }
        public bool IsExitRow { get; set; }
        public bool HasExtraLegroom { get; set; }
        public decimal ExtraCharge { get; set; } = 0;
    }

    public class BulkCreateSeatsDto
    {
        [Required]
        public int AircraftId { get; set; }

        [Required]
        public int EconomyRows { get; set; }

        public int PremiumEconomyRows { get; set; } = 0;
        public int BusinessRows { get; set; } = 0;
        public int FirstClassRows { get; set; } = 0;

        [Required]
        public List<string> Columns { get; set; } = []; // ["A", "B", "C", "D", "E", "F"]

        public List<int> ExitRows { get; set; } = [];
        public List<string> WindowColumns { get; set; } = ["A", "F"];
        public List<string> AisleColumns { get; set; } = ["C", "D"];
    }

    public class BlockSeatsRequestDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public List<string> SeatNumbers { get; set; } = [];

        public int BlockDurationMinutes { get; set; } = 10;
    }

    public class ReleaseSeatsRequestDto
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public List<string> SeatNumbers { get; set; } = [];
    }
}
