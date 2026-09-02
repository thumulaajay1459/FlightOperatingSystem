using System.Text;
using System.Text.Json;

namespace AIService.RAG
{
    public class ResponseGenerator
    {
        private readonly KnowledgeBase _kb;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ResponseGenerator(KnowledgeBase kb) => _kb = kb;

        public string Generate(string userMessage, string intent, string? liveData, List<string> history)
        {
            var sb = new StringBuilder();

            // Retrieve relevant knowledge chunks
            var chunks = _kb.Retrieve(userMessage, topK: 3);

            switch (intent)
            {
                case "greeting":
                    return GenerateGreeting(history);

                case "flights":
                    return GenerateFlightResponse(userMessage, liveData, chunks);

                case "routes":
                    return GenerateRoutesResponse(liveData);

                case "bookings":
                    return GenerateBookingResponse(userMessage, liveData, chunks);

                case "seats":
                    return GenerateSeatResponse(userMessage, liveData, chunks);

                case "payment":
                    return GeneratePaymentResponse(chunks);

                case "account":
                    return GenerateAccountResponse(chunks);

                case "offtopic":
                    return "I can only assist with flight booking related questions for FlightOps. How can I help you with flights, bookings, or seats?";

                default:
                    return GenerateGeneralResponse(userMessage, chunks);
            }
        }

        private static string GenerateGreeting(List<string> history)
        {
            if (history.Count == 0)
                return "Hello! Welcome to FlightOps. I can help you with:\n" +
                       "- ✈️ Searching available flights\n" +
                       "- 🎫 Managing your bookings\n" +
                       "- 💺 Seat selection\n" +
                       "- 💳 Payment information\n\n" +
                       "What would you like to know?";

            return "How can I help you further?";
        }

        private static string GenerateRoutesResponse(string? liveData)
        {
            if (!string.IsNullOrEmpty(liveData) && liveData != "Route data unavailable.")
            {
                try
                {
                    var routes = JsonSerializer.Deserialize<List<JsonElement>>(liveData, _json);
                    if (routes != null && routes.Count > 0)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"Here are the available routes ({routes.Count} found):\n");
                        foreach (var r in routes)
                        {
                            var origin = r.TryGetProperty("origin", out var o) ? o.GetString() : "N/A";
                            var dest   = r.TryGetProperty("destination", out var d) ? d.GetString() : "N/A";
                            var dist   = r.TryGetProperty("distanceKm", out var dk) ? dk.GetInt32().ToString() : "N/A";
                            sb.AppendLine($"🛣️ {origin} → {dest} ({dist} km)");
                        }
                        return sb.ToString();
                    }
                }
                catch { }
            }

            return "FlightOps operates routes between Delhi, Mumbai, Bangalore, Chennai, and Hyderabad. Route data is currently unavailable. Please try again later.";
        }

        private static string GenerateFlightResponse(string query, string? liveData, List<VectorDocument> chunks)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(liveData) && liveData != "Flight data temporarily unavailable.")
            {
                try
                {
                    var response = JsonSerializer.Deserialize<JsonElement>(liveData, _json);
                    
                    // Check if it's a search response with outbound/return structure
                    if (response.TryGetProperty("outbound", out var outbound))
                    {
                        if (outbound.TryGetProperty("flights", out var outboundFlights))
                        {
                            var flights = outboundFlights.EnumerateArray().ToList();
                            if (flights.Count > 0)
                            {
                                sb.AppendLine($"✈️ **Outbound Flights** ({flights.Count} found):\n");
                                foreach (var f in flights.Take(5))
                                {
                                    FormatFlightSearchResult(sb, f);
                                }
                                if (flights.Count > 5)
                                    sb.AppendLine($"\n...and {flights.Count - 5} more flights available.");
                            }
                        }

                        // Check for return flights
                        if (response.TryGetProperty("returnTrip", out var returnTrip) && returnTrip.ValueKind != JsonValueKind.Null)
                        {
                            if (returnTrip.TryGetProperty("flights", out var returnFlights))
                            {
                                var flights = returnFlights.EnumerateArray().ToList();
                                if (flights.Count > 0)
                                {
                                    sb.AppendLine($"\n🔄 **Return Flights** ({flights.Count} found):\n");
                                    foreach (var f in flights.Take(5))
                                    {
                                        FormatFlightSearchResult(sb, f);
                                    }
                                    if (flights.Count > 5)
                                        sb.AppendLine($"\n...and {flights.Count - 5} more flights available.");
                                }
                            }
                        }

                        return sb.ToString();
                    }
                    // Check if it's a paginated response
                    else if (response.TryGetProperty("data", out var data))
                    {
                        var flights = data.EnumerateArray().ToList();
                        if (flights.Count > 0)
                        {
                            sb.AppendLine($"Here are the available flights ({flights.Count} found):\n");
                            foreach (var f in flights.Take(5))
                            {
                                var num = f.TryGetProperty("flightNumber", out var fn) ? fn.GetString() : "N/A";
                                var origin = f.TryGetProperty("origin", out var o) ? o.GetString() : "N/A";
                                var dest = f.TryGetProperty("destination", out var d) ? d.GetString() : "N/A";
                                var dep = f.TryGetProperty("departureTime", out var dt) ? dt.GetString() : "N/A";
                                var status = f.TryGetProperty("status", out var s) ? s.GetString() : "N/A";
                                var seats = f.TryGetProperty("totalSeats", out var ts) ? ts.GetInt32().ToString() : "N/A";

                                sb.AppendLine($"✈️ **{num}** | {origin} → {dest}");
                                sb.AppendLine($"   Departure: {dep} | Status: {status} | Seats: {seats}\n");
                            }

                            if (flights.Count > 5)
                                sb.AppendLine($"...and {flights.Count - 5} more flights available.");

                            return sb.ToString();
                        }
                    }
                    // Direct array of flights
                    else if (response.ValueKind == JsonValueKind.Array)
                    {
                        var flights = response.EnumerateArray().ToList();
                        if (flights.Count > 0)
                        {
                            sb.AppendLine($"Here are the available flights ({flights.Count} found):\n");
                            foreach (var f in flights.Take(5))
                            {
                                FormatFlightSearchResult(sb, f);
                            }

                            if (flights.Count > 5)
                                sb.AppendLine($"\n...and {flights.Count - 5} more flights available.");

                            return sb.ToString();
                        }
                    }
                }
                catch { }
            }

            // Fallback — only return flight-specific knowledge chunks
            var flightChunks = chunks.Where(c => c.Category == "faq" && c.Id.StartsWith("flight")).ToList();
            if (flightChunks.Count > 0)
                return string.Join("\n\n", flightChunks.Select(c => c.Content));

            return "FlightService is currently unavailable. Please try again later or use GET /flights to view available flights directly.";
        }

        private static void FormatFlightSearchResult(StringBuilder sb, JsonElement flight)
        {
            var num = flight.TryGetProperty("flightNumber", out var fn) ? fn.GetString() : "N/A";
            var origin = flight.TryGetProperty("origin", out var o) ? o.GetString() : "N/A";
            var originCode = flight.TryGetProperty("originCode", out var oc) ? oc.GetString() : "";
            var dest = flight.TryGetProperty("destination", out var d) ? d.GetString() : "N/A";
            var destCode = flight.TryGetProperty("destinationCode", out var dc) ? dc.GetString() : "";
            var dep = flight.TryGetProperty("departureTime", out var dt) ? DateTime.Parse(dt.GetString()!).ToString("MMM dd, HH:mm") : "N/A";
            var arr = flight.TryGetProperty("arrivalTime", out var at) ? DateTime.Parse(at.GetString()!).ToString("HH:mm") : "N/A";
            var duration = flight.TryGetProperty("durationHours", out var dh) ? $"{dh.GetDouble():F1}h" : "N/A";
            var available = flight.TryGetProperty("availableSeats", out var av) ? av.GetInt32().ToString() : "N/A";
            var economy = flight.TryGetProperty("economyPrice", out var ep) ? $"₹{ep.GetDecimal():N0}" : "N/A";
            var business = flight.TryGetProperty("businessPrice", out var bp) ? $"₹{bp.GetDecimal():N0}" : "N/A";

            sb.AppendLine($"✈️ **{num}** | {origin} ({originCode}) → {dest} ({destCode})");
            sb.AppendLine($"   🕐 {dep} - {arr} ({duration}) | 💺 {available} seats");
            sb.AppendLine($"   💰 Economy: {economy} | Business: {business}\n");
        }

        private static string GenerateBookingResponse(string query, string? liveData, List<VectorDocument> chunks)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(liveData))
            {
                try
                {
                    var bookings = JsonSerializer.Deserialize<List<JsonElement>>(liveData, _json);
                    if (bookings != null && bookings.Count > 0)
                    {
                        sb.AppendLine($"You have {bookings.Count} booking(s):\n");
                        foreach (var b in bookings)
                        {
                            var ref_   = b.TryGetProperty("bookingReference", out var br) ? br.GetString() : "N/A";
                            var status = b.TryGetProperty("bookingStatus", out var bs) ? bs.GetString() : "N/A";
                            var amount = b.TryGetProperty("totalAmount", out var ta) ? ta.GetDecimal().ToString("C") : "N/A";
                            var date   = b.TryGetProperty("createdAt", out var ca) ? ca.GetString() : "N/A";

                            sb.AppendLine($"📋 **{ref_}**");
                            sb.AppendLine($"   Status: {status} | Amount: {amount} | Date: {date}\n");
                        }
                        return sb.ToString();
                    }
                    else
                    {
                        return "You don't have any bookings yet. Would you like to search for available flights?";
                    }
                }
                catch { }
            }

            var bookingChunks = chunks.Where(c => c.Id.StartsWith("faq")).ToList();
            if (bookingChunks.Count > 0)
                return string.Join("\n\n", bookingChunks.Select(c => c.Content));

            return "I couldn't retrieve your booking information. Please make sure you're logged in.";
        }

        private static string GenerateSeatResponse(string query, string? liveData, List<VectorDocument> chunks)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(liveData) && liveData.Contains("seatNumber", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var seatMap = JsonSerializer.Deserialize<JsonElement>(liveData, _json);
                    if (seatMap.TryGetProperty("seats", out var seats))
                    {
                        var seatList = seats.EnumerateArray().ToList();
                        var available = seatList.Count(s => s.TryGetProperty("status", out var st) && st.GetString() == "Available");
                        var locked    = seatList.Count(s => s.TryGetProperty("status", out var st) && st.GetString() == "Locked");
                        var confirmed = seatList.Count(s => s.TryGetProperty("status", out var st) && st.GetString() == "Confirmed");

                        sb.AppendLine($"💺 Seat Map Summary:");
                        sb.AppendLine($"   ✅ Available: {available}");
                        sb.AppendLine($"   🔒 Locked: {locked}");
                        sb.AppendLine($"   ✔️ Confirmed: {confirmed}\n");

                        var availableSeats = seatList
                            .Where(s => s.TryGetProperty("status", out var st) && st.GetString() == "Available")
                            .Take(10)
                            .Select(s => s.TryGetProperty("seatNumber", out var sn) ? sn.GetString() : "")
                            .Where(s => !string.IsNullOrEmpty(s));

                        sb.AppendLine($"Available seats (first 10): {string.Join(", ", availableSeats)}");
                        return sb.ToString();
                    }
                }
                catch { }
            }

            if (chunks.Count > 0)
                return string.Join("\n\n", chunks.Where(c => c.Id.StartsWith("faq") || c.Id.StartsWith("seat")).Select(c => c.Content));

            return "Please specify a flight ID to view seat availability. Example: 'show seats for flight 1'";
        }

        private static string GeneratePaymentResponse(List<VectorDocument> chunks)
        {
            var payChunks = chunks.Where(c => c.Id.StartsWith("pay")).ToList();
            if (payChunks.Count > 0)
                return string.Join("\n\n", payChunks.Select(c => c.Content));

            return "Payments are processed through Razorpay. Create a payment order, complete the payment, then verify it to confirm your booking.";
        }

        private static string GenerateAccountResponse(List<VectorDocument> chunks)
        {
            var accChunks = chunks.Where(c => c.Id.StartsWith("acc")).ToList();
            if (accChunks.Count > 0)
                return string.Join("\n\n", accChunks.Select(c => c.Content));

            return "For account help, use POST /auth/register to create an account or POST /auth/login to get your JWT token.";
        }

        private static string GenerateGeneralResponse(string query, List<VectorDocument> chunks)
        {
            if (chunks.Count > 0)
                return string.Join("\n\n", chunks.Select(c => c.Content));

            return "I'm not sure I understand your question. I can help you with:\n" +
                   "- Flight search and schedules\n" +
                   "- Booking management\n" +
                   "- Seat selection\n" +
                   "- Payment information\n\n" +
                   "Please ask a specific question about any of these topics.";
        }
    }
}
