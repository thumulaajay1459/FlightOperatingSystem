using BookingService.Interfaces;
using System.Text.Json;

namespace BookingService.Services
{
    public class FlightServiceClient : IFlightServiceClient
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public FlightServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> FlightExistsAsync(int flightId)
        {
            var response = await _httpClient.GetAsync($"/api/ScheduledFlights/{flightId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> FlightIsActiveAsync(int flightId)
        {
            var flight = await GetFlightDetailsAsync(flightId);
            return flight?.Status == "Scheduled" || flight?.Status == "Delayed";
        }

        public async Task<int> GetAvailableSeatsAsync(int flightId)
        {
            var flight = await GetFlightDetailsAsync(flightId);
            return flight?.TotalSeats ?? 0;
        }

        public async Task<FlightDto?> GetFlightDetailsAsync(int flightId)
        {
            var response = await _httpClient.GetAsync($"/api/ScheduledFlights/{flightId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FlightDto>(content, _jsonOptions);
        }

        // Matches FlightSummaryDto returned by FlightService's GetFlightById
        public class FlightDto
        {
            public int FlightId { get; set; }
            public string FlightNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int TotalSeats { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public string Origin { get; set; } = string.Empty;
            public string Destination { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public int DistanceKm { get; set; }
        }
    }
}
