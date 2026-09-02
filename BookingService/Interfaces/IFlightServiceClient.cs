namespace BookingService.Interfaces
{
    public interface IFlightServiceClient
    {
        Task<bool> FlightExistsAsync(int flightId);
        Task<bool> FlightIsActiveAsync(int flightId);
        Task<int> GetAvailableSeatsAsync(int flightId);
    }
}
