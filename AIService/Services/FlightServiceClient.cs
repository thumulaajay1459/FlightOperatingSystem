using System.Text.Json;

namespace AIService.Services
{
    public class FlightServiceClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public FlightServiceClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<string> GetAllFlightsAsync()
        {
            var response = await _httpClient.GetAsync("/api/ScheduledFlights");
            if (!response.IsSuccessStatusCode) return "Flight data unavailable.";
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SearchFlightsAsync(string? from = null, string? to = null, DateTime? date = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(from)) query.Add($"from={Uri.EscapeDataString(from)}");
            if (!string.IsNullOrEmpty(to)) query.Add($"to={Uri.EscapeDataString(to)}");
            if (date.HasValue) query.Add($"date={date.Value:yyyy-MM-dd}");

            var url = "/api/ScheduledFlights/search-flights" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return "Flight search unavailable.";
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetFlightByIdAsync(int flightId)
        {
            var response = await _httpClient.GetAsync($"/api/ScheduledFlights/{flightId}");
            if (!response.IsSuccessStatusCode) return $"Flight {flightId} not found.";
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetRoutesAsync()
        {
            var response = await _httpClient.GetAsync("/api/Routes");
            if (!response.IsSuccessStatusCode) return string.Empty;
            return await response.Content.ReadAsStringAsync();
        }
    }
}
