using FlightService.Data;
using FlightService.DTOs;
using FlightService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Services;

public class FlightSearchService : IFlightSearchService
{
    private readonly appDataContext _context;

    public FlightSearchService(appDataContext context)
    {
        _context = context;
    }

    // Smart Airport Autocomplete with Fuzzy Matching
    public async Task<List<AirportSuggestionDto>> GetAirportSuggestionsAsync(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return new();

        var q = query.ToUpper().Trim();

        var airports = await _context.Airports
            .Where(a =>
                a.Code.ToUpper().Contains(q) ||
                a.City.ToUpper().Contains(q) ||
                a.Name.ToUpper().Contains(q) ||
                a.Country.ToUpper().Contains(q))
            .Take(limit)
            .Select(a => new AirportSuggestionDto
            {
                AirportId = a.AirportId,
                Code = a.Code,
                Name = a.Name,
                City = a.City,
                Country = a.Country,
                DisplayText = $"{a.Code} - {a.City} - {a.Name}"
            })
            .ToListAsync();

        // Prioritize exact IATA code matches
        return airports
            .OrderByDescending(a => a.Code.Equals(q, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(a => a.Code.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(a => a.City.StartsWith(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Main Search with Alternate Suggestions
    public async Task<FlightSearchResultDto> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        var result = new FlightSearchResultDto();

        // Resolve origin/destination to airport IDs
        var originAirport = await ResolveAirportAsync(request.Origin);
        var destAirport = await ResolveAirportAsync(request.Destination);

        if (originAirport == null || destAirport == null)
        {
            result.Alternatives = new AlternateSuggestionsDto
            {
                Message = "Invalid origin or destination. Please use airport codes or city names."
            };
            return result;
        }

        // Search outbound flights
        result.OutboundFlights = await SearchFlightsInternalAsync(
            originAirport.AirportId,
            destAirport.AirportId,
            request.DepartureDate,
            request);

        // Search return flights
        if (request.ReturnDate.HasValue)
        {
            result.ReturnFlights = await SearchFlightsInternalAsync(
                destAirport.AirportId,
                originAirport.AirportId,
                request.ReturnDate,
                request);
        }

        // Metadata
        result.Metadata = new SearchMetadataDto
        {
            Origin = $"{originAirport.City} ({originAirport.Code})",
            Destination = $"{destAirport.City} ({destAirport.Code})",
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            TripType = request.ReturnDate.HasValue ? "RoundTrip" : "OneWay",
            TotalResults = result.OutboundFlights.Count
        };

        // Alternate suggestions if no flights found
        if (result.OutboundFlights.Count == 0 && request.DepartureDate.HasValue)
        {
            result.Alternatives = await GetAlternateSuggestionsAsync(
                originAirport.AirportId,
                destAirport.AirportId,
                request.DepartureDate.Value);
            result.Metadata.HasAlternatives = true;
        }

        return result;
    }

    // Get Alternate Flights (±3 days, cheaper options)
    public async Task<List<AlternateFlightDto>> GetAlternateFlightsAsync(int originId, int destinationId, DateTime date)
    {
        var alternates = new List<AlternateFlightDto>();

        // Search ±3 days
        for (int i = -3; i <= 3; i++)
        {
            if (i == 0) continue;

            var altDate = date.AddDays(i);
            var flights = await GetFlightsForDateAsync(originId, destinationId, altDate);

            foreach (var flight in flights.Take(2))
            {
                alternates.Add(new AlternateFlightDto
                {
                    FlightId = flight.FlightId,
                    FlightNumber = flight.FlightNumber,
                    DepartureTime = flight.DepartureTime,
                    Price = flight.EconomyPrice,
                    Reason = i < 0 ? $"{Math.Abs(i)} day(s) earlier" : $"{i} day(s) later"
                });
            }
        }

        return alternates.OrderBy(a => a.Price).Take(6).ToList();
    }

    // Private Helpers
    private async Task<Airport?> ResolveAirportAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return null;

        var q = query.ToUpper().Trim();

        return await _context.Airports
            .FirstOrDefaultAsync(a =>
                a.Code.ToUpper() == q ||
                a.City.ToUpper() == q ||
                a.Name.ToUpper().Contains(q));
    }

    private async Task<List<FlightOptionDto>> SearchFlightsInternalAsync(
        int originId, int destId, DateTime? date, FlightSearchRequestDto request)
    {
        var query = _context.Flights
            .Include(f => f.Aircraft)
            .Include(f => f.Route)
                .ThenInclude(r => r!.Origin)
            .Include(f => f.Route)
                .ThenInclude(r => r!.Destination)
            .Where(f => f.Status == "Scheduled")
            .Where(f => f.Route!.OriginAirportId == originId && f.Route.DestinationAirportId == destId)
            .AsNoTracking();

        if (date.HasValue)
            query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
        else
            query = query.Where(f => f.DepartureTime >= DateTime.UtcNow);

        if (!string.IsNullOrEmpty(request.TimePreference))
        {
            query = request.TimePreference.ToLower() switch
            {
                "morning" => query.Where(f => f.DepartureTime.Hour >= 6 && f.DepartureTime.Hour < 12),
                "afternoon" => query.Where(f => f.DepartureTime.Hour >= 12 && f.DepartureTime.Hour < 18),
                "evening" => query.Where(f => f.DepartureTime.Hour >= 18 && f.DepartureTime.Hour < 24),
                _ => query
            };
        }

        var flights = await query.ToListAsync();

        // Get seat availability
        var flightIds = flights.Select(f => f.FlightId).ToList();
        var bookedSeats = await _context.Set<FlightSeat>()
            .Where(fs => flightIds.Contains(fs.FlightId) &&
                  (fs.Status == SeatStatus.Booked ||
                   (fs.Status == SeatStatus.Blocked && fs.BlockedUntil > DateTime.UtcNow)))
            .GroupBy(fs => fs.FlightId)
            .Select(g => new { FlightId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FlightId, x => x.Count);

        var results = flights.Select(f =>
        {
            var totalSeats = f.Aircraft?.TotalSeats ?? 0;
            var booked = bookedSeats.GetValueOrDefault(f.FlightId, 0);
            var available = totalSeats - booked;

            return new FlightOptionDto
            {
                FlightId = f.FlightId,
                FlightNumber = f.FlightNumber,
                Origin = f.Route?.Origin?.City ?? "",
                OriginCode = f.Route?.Origin?.Code ?? "",
                Destination = f.Route?.Destination?.City ?? "",
                DestinationCode = f.Route?.Destination?.Code ?? "",
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Duration = FormatDuration(f.DepartureTime, f.ArrivalTime),
                Aircraft = $"{f.Aircraft?.Manufacturer} {f.Aircraft?.Model}",
                AvailableSeats = available,
                Pricing = new PricingDto
                {
                    Economy = f.EconomyPrice,
                    PremiumEconomy = f.PremiumEconomyPrice,
                    Business = f.BusinessPrice,
                    FirstClass = f.FirstClassPrice
                }
            };
        }).ToList();

        // Apply filters
        if (request.MaxPrice.HasValue)
            results = results.Where(f => f.Pricing.Economy <= request.MaxPrice.Value).ToList();

        // Sort
        results = request.SortBy?.ToLower() switch
        {
            "price" => results.OrderBy(f => f.Pricing.Economy).ToList(),
            "duration" => results.OrderBy(f => f.DepartureTime).ThenBy(f => f.ArrivalTime).ToList(),
            "departure" => results.OrderBy(f => f.DepartureTime).ToList(),
            _ => results.OrderBy(f => f.Pricing.Economy).ToList()
        };

        return results;
    }

    private async Task<AlternateSuggestionsDto> GetAlternateSuggestionsAsync(int originId, int destId, DateTime date)
    {
        var nearbyDates = await GetAlternateFlightsAsync(originId, destId, date);

        return new AlternateSuggestionsDto
        {
            NearbyDates = nearbyDates,
            Message = nearbyDates.Any()
                ? "No flights found for your selected date. Here are alternatives:"
                : "No flights available for this route in the next week."
        };
    }

    private async Task<List<Flight>> GetFlightsForDateAsync(int originId, int destId, DateTime date)
    {
        return await _context.Flights
            .Where(f => f.Status == "Scheduled")
            .Where(f => f.Route!.OriginAirportId == originId && f.Route.DestinationAirportId == destId)
            .Where(f => f.DepartureTime.Date == date.Date)
            .OrderBy(f => f.EconomyPrice)
            .Take(3)
            .ToListAsync();
    }

    private static string FormatDuration(DateTime departure, DateTime arrival)
    {
        var duration = arrival - departure;
        return $"{duration.Hours}h {duration.Minutes}m";
    }
}

public interface IFlightSearchService
{
    Task<List<AirportSuggestionDto>> GetAirportSuggestionsAsync(string query, int limit = 10);
    Task<FlightSearchResultDto> SearchFlightsAsync(FlightSearchRequestDto request);
    Task<List<AlternateFlightDto>> GetAlternateFlightsAsync(int originId, int destinationId, DateTime date);
}
