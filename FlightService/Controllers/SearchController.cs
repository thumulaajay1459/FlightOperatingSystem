using Microsoft.AspNetCore.Mvc;
using FlightService.Services;
using FlightService.DTOs;

namespace FlightService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IFlightSearchService _searchService;

    public SearchController(IFlightSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Autocomplete airport suggestions
    /// GET /api/search/airports?q=hyd
    /// </summary>
    [HttpGet("airports")]
    public async Task<IActionResult> GetAirportSuggestions([FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<AirportSuggestionDto>());

        var suggestions = await _searchService.GetAirportSuggestionsAsync(q, limit);
        return Ok(suggestions);
    }

    /// <summary>
    /// Smart flight search with alternate suggestions
    /// POST /api/search/flights
    /// </summary>
    [HttpPost("flights")]
    public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
            return BadRequest(new { error = "Origin and destination are required" });

        if (request.DepartureDate.HasValue && request.DepartureDate.Value.Date < DateTime.UtcNow.Date)
            return BadRequest(new { error = "Departure date cannot be in the past" });

        if (request.ReturnDate.HasValue && request.DepartureDate.HasValue &&
            request.ReturnDate.Value.Date < request.DepartureDate.Value.Date)
            return BadRequest(new { error = "Return date must be after departure date" });

        var result = await _searchService.SearchFlightsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get alternate flight options (±3 days)
    /// GET /api/search/alternates?originId=1&destId=2&date=2024-12-25
    /// </summary>
    [HttpGet("alternates")]
    public async Task<IActionResult> GetAlternateFlights(
        [FromQuery] int originId,
        [FromQuery] int destId,
        [FromQuery] DateTime date)
    {
        var alternates = await _searchService.GetAlternateFlightsAsync(originId, destId, date);
        return Ok(alternates);
    }
}
