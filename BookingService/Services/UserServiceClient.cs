using System.Text.Json;

namespace BookingService.Services
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserDto?> GetUserAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/User/{userId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/api/UserProfile/{userId}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileDto>(content, _jsonOptions);
        }

        public class UserDto
        {
            public int ID { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class UserProfileDto
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PassportNumber { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Nationality { get; set; } = string.Empty;
        }
    }
}
