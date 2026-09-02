namespace FlightService.DTOs;

// Airport Autocomplete
public class AirportSuggestionDto
{
    public int AirportId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty; // "HYD - Hyderabad - Rajiv Gandhi International"
}

// Search Request
public class FlightSearchRequestDto
{
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int Infants { get; set; } = 0;
    public string? CabinClass { get; set; } = "Economy";
    public string? SortBy { get; set; } = "price";
    public decimal? MaxPrice { get; set; }
    public string? TimePreference { get; set; }
    public bool DirectFlightsOnly { get; set; } = false;
}

// Search Result
public class FlightSearchResultDto
{
    public SearchMetadataDto Metadata { get; set; } = new();
    public List<FlightOptionDto> OutboundFlights { get; set; } = new();
    public List<FlightOptionDto>? ReturnFlights { get; set; }
    public AlternateSuggestionsDto? Alternatives { get; set; }
}

public class SearchMetadataDto
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string TripType { get; set; } = "OneWay";
    public int TotalResults { get; set; }
    public bool HasAlternatives { get; set; }
}

public class FlightOptionDto
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Aircraft { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
    public PricingDto Pricing { get; set; } = new();
}

public class PricingDto
{
    public decimal Economy { get; set; }
    public decimal PremiumEconomy { get; set; }
    public decimal Business { get; set; }
    public decimal FirstClass { get; set; }
}

// Alternate Suggestions
public class AlternateSuggestionsDto
{
    public List<AlternateFlightDto>? NearbyDates { get; set; }
    public List<AlternateFlightDto>? CheaperOptions { get; set; }
    public string? Message { get; set; }
}

public class AlternateFlightDto
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal Price { get; set; }
    public string Reason { get; set; } = string.Empty; // "1 day earlier", "₹500 cheaper"
}
