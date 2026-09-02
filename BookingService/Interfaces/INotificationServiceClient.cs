using BookingService.DTOs;

namespace BookingService.Interfaces
{
    public interface INotificationServiceClient
    {
        Task SendTicketConfirmationAsync(TicketConfirmationDto request);
    }
}
