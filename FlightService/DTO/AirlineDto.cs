namespace FlightService.DTO
{
    public class AirlineDto
    {
        public int AirlineId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAirlineDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
    }

    public class UpdateAirlineDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }
        public bool IsActive { get; set; }
    }
}
