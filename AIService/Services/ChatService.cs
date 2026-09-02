using AIService.Data;
using AIService.Models;
using AIService.RAG;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace AIService.Services
{
    public class ChatService
    {
        private readonly AiDbContext _db;
        private readonly FlightServiceClient _flightClient;
        private readonly BookingServiceClient _bookingClient;
        private readonly SeatServiceClient _seatClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IMemoryCache _cache;
        private readonly KnowledgeBase _kb;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private const string SystemPrompt =
            "You are a flight booking assistant ONLY for FlightOps application. " +
            "You ONLY answer questions about: flights, routes, bookings, seats, payments, and user accounts within FlightOps. " +
            "If the user asks anything outside these topics, respond with: " +
            "'I can only assist with FlightOps flight booking related questions.' " +
            "Only use the data provided in the context. Do not make up flight numbers, prices, or booking references. " +
            "Be concise and friendly.";

        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ChatService(
            AiDbContext db,
            FlightServiceClient flightClient,
            BookingServiceClient bookingClient,
            SeatServiceClient seatClient,
            ILogger<ChatService> logger,
            IMemoryCache cache,
            KnowledgeBase kb,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _db = db;
            _flightClient = flightClient;
            _bookingClient = bookingClient;
            _seatClient = seatClient;
            _logger = logger;
            _cache = cache;
            _kb = kb;
            _httpClient = httpClientFactory.CreateClient("AzureAI");
            _config = config;
        }

        public async Task<string> ChatAsync(int sessionId, string userMessage, string userId)
        {
            // Load last 5 messages
            var history = await _db.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // Detect intent
            var intent = DetectIntent(userMessage.ToLower());

            // Early return for off-topic
            if (intent == "offtopic")
                return "I can only assist with FlightOps flight booking related questions.";

            // RAG: fetch live data + knowledge base chunks
            var liveData  = await FetchLiveDataAsync(intent, userMessage.ToLower(), userId);
            var kbChunks  = _kb.Retrieve(userMessage, topK: 3);

            // Build system content with context
            var contextSb = new StringBuilder();
            if (kbChunks.Count > 0)
            {
                contextSb.AppendLine("--- KNOWLEDGE BASE ---");
                foreach (var chunk in kbChunks)
                    contextSb.AppendLine(chunk.Content);
            }
            if (!string.IsNullOrEmpty(liveData))
            {
                contextSb.AppendLine("--- LIVE SYSTEM DATA ---");
                contextSb.AppendLine(liveData);
            }

            var systemContent = contextSb.Length > 0
                ? $"{SystemPrompt}\n\n{contextSb}"
                : SystemPrompt;

            // Build messages
            var messages = new List<object>
            {
                new { role = "system", content = systemContent }
            };

            foreach (var msg in history)
                messages.Add(new { role = msg.Role == MessageRole.User ? "user" : "assistant", content = msg.Content });

            messages.Add(new { role = "user", content = userMessage });

            // Call Azure AI
            try
            {
                var requestBody = new
                {
                    model       = _config["AzureAI:Model"] ?? "Phi-4-mini-instruct",
                    messages    = messages,
                    temperature = 0.5,
                    max_tokens  = 1024
                };

                var apiKey  = _config["AzureAI:ApiKey"]!;
                var request = new HttpRequestMessage(HttpMethod.Post, _config["AzureAI:Endpoint"]!);
                request.Headers.Add("api-key", apiKey);
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response     = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Azure AI status: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Azure AI error: {Body}", responseBody);
                    return $"AI error {response.StatusCode}: {responseBody}";
                }

                var result = JsonSerializer.Deserialize<AzureAIResponse>(responseBody, _json);
                return result?.Choices?[0]?.Message?.Content
                    ?? "I could not generate a response. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure AI call failed: {Message}", ex.Message);
                return $"AI error: {ex.Message}";
            }
        }

        private async Task<string?> FetchLiveDataAsync(string intent, string message, string userId)
        {
            switch (intent)
            {
                case "flights":
                    // Try to extract location and date from message
                    var (from, to, date) = ExtractFlightSearchParams(message);
                    _logger.LogInformation($"Extracted search params - From: {from}, To: {to}, Date: {date}");
                    
                    if (!string.IsNullOrEmpty(from) || !string.IsNullOrEmpty(to) || date.HasValue)
                    {
                        try 
                        { 
                            var result = await _flightClient.SearchFlightsAsync(from, to, date);
                            _logger.LogInformation($"Flight search result length: {result?.Length ?? 0}");
                            return result;
                        }
                        catch (Exception ex) 
                        { 
                            _logger.LogError(ex, "Flight search failed: {Message}", ex.Message);
                        }
                    }
                    
                    try
                    {
                        var allFlights = await GetCachedFlightsAsync();
                        _logger.LogInformation($"All flights result length: {allFlights?.Length ?? 0}");
                        return allFlights;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Get all flights failed: {Message}", ex.Message);
                        return null;
                    }

                case "routes":
                    try { return await _flightClient.GetRoutesAsync(); }
                    catch (Exception ex) { _logger.LogWarning("Routes unavailable: {M}", ex.Message); return null; }

                case "bookings":
                    if (userId == "test-user-1" || string.IsNullOrEmpty(userId))
                    {
                        return "User needs to login to access booking information.";
                    }
                    try { return await _bookingClient.GetUserBookingsAsync(userId); }
                    catch (Exception ex) { _logger.LogWarning("BookingService unavailable: {M}", ex.Message); return null; }

                case "seats":
                    var flightId = ExtractFlightId(message);
                    if (flightId.HasValue)
                    {
                        try { return await _seatClient.GetSeatMapAsync(flightId.Value); }
                        catch (Exception ex) { _logger.LogWarning("SeatService unavailable: {M}", ex.Message); return null; }
                    }
                    return null;

                default:
                    return null;
            }
        }

        private static string DetectIntent(string message)
        {
            var offTopicKeywords = new[] { "weather", "cricket", "movie", "recipe", "python", "java", "math", "capital", "president", "sport", "stock", "bitcoin" };
            if (offTopicKeywords.Any(message.Contains))
                return "offtopic";

            if (message.Length < 15 || message.StartsWith("hi") || message.StartsWith("hello")
                || message.StartsWith("hey") || message.StartsWith("thanks") || message.StartsWith("thank"))
                return "greeting";

            if (message.Contains("payment") || message.Contains("pay") || message.Contains("razorpay")
                || message.Contains("refund") || message.Contains("price") || message.Contains("cost"))
                return "payment";

            if (message.Contains("register") || message.Contains("login") || message.Contains("account")
                || message.Contains("password") || message.Contains("profile") || message.Contains("admin"))
                return "account";

            if (message.Contains("booking") || message.Contains("ticket") || message.Contains("reservation")
                || message.Contains("my trip") || message.Contains("cancel") || message.Contains("my booking"))
                return "bookings";

            if (message.Contains("seat") || message.Contains("window") || message.Contains("aisle")
                || message.Contains("business class") || message.Contains("economy class"))
                return "seats";

            if (message.Contains("route") || message.Contains("routes") || message.Contains("destination")
                || message.Contains("between"))
                return "routes";

            if (message.Contains("flight") || message.Contains("schedule") || message.Contains("available")
                || message.Contains("travel") || message.Contains("airport") || message.Contains("delhi")
                || message.Contains("mumbai") || message.Contains("bangalore") || message.Contains("chennai")
                || message.Contains("hyderabad"))
                return "flights";

            return "general";
        }

        private async Task<string> GetCachedFlightsAsync()
        {
            return await _cache.GetOrCreateAsync("all_flights", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                try { return await _flightClient.GetAllFlightsAsync(); }
                catch (Exception ex)
                {
                    _logger.LogWarning("FlightService unavailable: {M}", ex.Message);
                    return string.Empty;
                }
            }) ?? string.Empty;
        }

        private static int? ExtractFlightId(string message)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, @"flight\s*(?:id\s*)?[:#]?\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
                return id;
            return null;
        }

        private static (string? from, string? to, DateTime? date) ExtractFlightSearchParams(string message)
        {
            string? from = null, to = null;
            DateTime? date = null;

            // Extract locations - expanded list
            var cities = new[] { "delhi", "mumbai", "bangalore", "chennai", "hyderabad", "kolkata", "pune", "ahmedabad", "blr", "bom", "del", "maa", "hyd" };

            var messageLower = message.ToLower();
            
            // Try to find "from X to Y" pattern
            var fromToMatch = System.Text.RegularExpressions.Regex.Match(messageLower, @"from\s+([a-z]+)\s+to\s+([a-z]+)");
            if (fromToMatch.Success)
            {
                from = fromToMatch.Groups[1].Value;
                to = fromToMatch.Groups[2].Value;
            }
            else
            {
                // Look for city names or codes
                var foundCities = cities.Where(c => messageLower.Contains(c)).ToList();
                if (foundCities.Count >= 2)
                {
                    from = foundCities[0];
                    to = foundCities[1];
                }
                else if (foundCities.Count == 1)
                {
                    // Check if it's asking about flights "to" or "from"
                    if (messageLower.Contains($"to {foundCities[0]}"))
                        to = foundCities[0];
                    else if (messageLower.Contains($"from {foundCities[0]}"))
                        from = foundCities[0];
                }
            }

            // Extract date
            var datePatterns = new[]
            {
                @"(\d{4})-(\d{2})-(\d{2})",
                @"(\d{2})/(\d{2})/(\d{4})",
                @"on\s+(\d{1,2})\s+(january|february|march|april|may|june|july|august|september|october|november|december)",
                @"(today|tomorrow)"
            };

            foreach (var pattern in datePatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(messageLower, pattern);
                if (match.Success)
                {
                    if (match.Groups[0].Value == "today")
                        date = DateTime.UtcNow.Date;
                    else if (match.Groups[0].Value == "tomorrow")
                        date = DateTime.UtcNow.Date.AddDays(1);
                    else if (DateTime.TryParse(match.Groups[0].Value, out var parsedDate))
                        date = parsedDate;
                    break;
                }
            }

            return (from, to, date);
        }

        // Response models
        private class AzureAIResponse
        {
            public List<AzureAIChoice>? Choices { get; set; }
        }

        private class AzureAIChoice
        {
            public AzureAIMessage? Message { get; set; }
        }

        private class AzureAIMessage
        {
            public string? Content { get; set; }
        }
    }
}
