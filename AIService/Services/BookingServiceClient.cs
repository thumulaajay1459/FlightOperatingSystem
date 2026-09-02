namespace AIService.Services
{
    public class BookingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetUserBookingsAsync(string userId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

            var response = await _httpClient.GetAsync($"/api/booking/my-bookings");
            if (!response.IsSuccessStatusCode) return "Booking data unavailable.";
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetBookingByIdAsync(int bookingId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

            var response = await _httpClient.GetAsync($"/api/booking/{bookingId}");
            if (!response.IsSuccessStatusCode) return $"Booking {bookingId} not found.";
            return await response.Content.ReadAsStringAsync();
        }
    }
}
