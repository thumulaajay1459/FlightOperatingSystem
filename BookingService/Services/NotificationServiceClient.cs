using BookingService.DTOs;
using BookingService.Interfaces;

namespace BookingService.Services
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendTicketConfirmationAsync(TicketConfirmationDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Notification/ticket-confirmation", request);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Ticket confirmation sent for booking {BookingId}", request.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ticket confirmation for booking {BookingId}", request.BookingId);
            }
        }
    }
}
