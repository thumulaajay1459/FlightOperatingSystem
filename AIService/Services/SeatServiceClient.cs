namespace AIService.Services
{
    public class SeatServiceClient
    {
        private readonly HttpClient _httpClient;

        public SeatServiceClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<string> GetSeatMapAsync(int flightId)
        {
            var response = await _httpClient.GetAsync($"/api/seats/flight/{flightId}/seat-map");
            if (!response.IsSuccessStatusCode) return $"Seat map for flight {flightId} unavailable.";
            return await response.Content.ReadAsStringAsync();
        }
    }
}
