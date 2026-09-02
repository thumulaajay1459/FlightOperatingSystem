using BookingService.DTOs;
using BookingService.Interfaces;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISeatLockService _seatLockService;
        private readonly FlightServiceClient _flightClient;
        private readonly UserServiceClient _userClient;

        public BookingService(
            IBookingRepository bookingRepository,
            ISeatLockService seatLockService,
            FlightServiceClient flightClient,
            UserServiceClient userClient)
        {
            _bookingRepository = bookingRepository;
            _seatLockService = seatLockService;
            _flightClient = flightClient;
            _userClient = userClient;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingRequestDto request)
        {
            if (!await _flightClient.FlightExistsAsync(request.FlightId))
                throw new InvalidOperationException($"Flight {request.FlightId} does not exist.");

            if (!await _flightClient.FlightIsActiveAsync(request.FlightId))
                throw new InvalidOperationException($"Flight {request.FlightId} is not available for booking.");

            // Validate return flight for round trip
            if (request.BookingType == "RoundTrip")
            {
                if (!request.ReturnFlightId.HasValue)
                    throw new InvalidOperationException("ReturnFlightId is required for round trip bookings.");

                if (!await _flightClient.FlightExistsAsync(request.ReturnFlightId.Value))
                    throw new InvalidOperationException($"Return flight {request.ReturnFlightId.Value} does not exist.");

                if (!await _flightClient.FlightIsActiveAsync(request.ReturnFlightId.Value))
                    throw new InvalidOperationException($"Return flight {request.ReturnFlightId.Value} is not available for booking.");
            }

            // Auto-fill passenger from user profile
            if (request.UseProfileAsPassenger)
            {
                if (string.IsNullOrWhiteSpace(request.SeatNumber))
                    throw new InvalidOperationException("SeatNumber is required when using profile as passenger.");

                var profile = await _userClient.GetUserProfileAsync(userId)
                    ?? throw new InvalidOperationException(
                        "No travel profile found. Please create your profile at POST /api/UserProfile before booking.");

                request.Passengers =
                [
                    new PassengerDto
                    {
                        FullName = profile.FullName,
                        Age = profile.Age,
                        Gender = profile.Gender,
                        PassportNumber = profile.PassportNumber,
                        SeatNumber = request.SeatNumber
                    }
                ];
            }

            if (request.Passengers.Count == 0)
                throw new InvalidOperationException(
                    "At least one passenger is required. Either provide Passengers or set UseProfileAsPassenger=true.");

            var lockDuration = TimeSpan.FromMinutes(10);
            var lockedSeats = new List<string>();

            try
            {
                foreach (var passenger in request.Passengers)
                {
                    var locked = await _seatLockService.TryLockSeatAsync(
                        request.FlightId, passenger.SeatNumber, userId, lockDuration);

                    if (!locked)
                    {
                        foreach (var seat in lockedSeats)
                            await _seatLockService.ReleaseLockAsync(request.FlightId, seat, userId);

                        throw new InvalidOperationException(
                            $"Seat {passenger.SeatNumber} is currently unavailable. Please choose another seat.");
                    }

                    lockedSeats.Add(passenger.SeatNumber);
                }

                var booking = new Booking
                {
                    UserId = userId,
                    FlightId = request.FlightId,
                    ReturnFlightId = request.ReturnFlightId,
                    BookingReference = GenerateBookingReference(),
                    BookingStatus = BookingStatus.Pending, // Changed to Pending until payment
                    BookingType = Enum.Parse<BookingType>(request.BookingType, ignoreCase: true),
                    TotalAmount = request.TotalAmount,
                    CreatedAt = DateTime.UtcNow,
                    Passengers = request.Passengers.Select(p => new Passenger
                    {
                        FullName = p.FullName,
                        Age = p.Age,
                        Gender = Enum.Parse<Gender>(p.Gender, ignoreCase: true),
                        PassengerType = Enum.Parse<PassengerType>(p.PassengerType, ignoreCase: true),
                        PassportNumber = p.PassportNumber,
                        SeatNumber = p.SeatNumber
                    }).ToList()
                };

                var created = await _bookingRepository.CreateAsync(booking);

                foreach (var seat in lockedSeats)
                    await _seatLockService.ReleaseLockAsync(request.FlightId, seat, userId);

                return MapToResponse(created);
            }
            catch
            {
                foreach (var seat in lockedSeats)
                    await _seatLockService.ReleaseLockAsync(request.FlightId, seat, userId);
                throw;
            }
        }

        public async Task<BookingDetailsDto?> GetBookingByIdAsync(int id, string userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null) return null;

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to view this booking.");

            return MapToDetails(booking);
        }

        public async Task<List<BookingDetailsDto>> GetBookingsByUserIdAsync(string userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            var result = new List<BookingDetailsDto>();
            
            foreach (var booking in bookings)
            {
                var dto = await MapToDetailsWithFlightInfoAsync(booking);
                result.Add(dto);
            }
            
            return result;
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int id, string userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Booking {id} not found.");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");

            if (booking.BookingStatus == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            if (booking.BookingStatus == BookingStatus.Completed)
                throw new InvalidOperationException("Completed bookings cannot be cancelled.");

            booking.BookingStatus = BookingStatus.Cancelled;
            await _bookingRepository.SaveChangesAsync();

            return MapToResponse(booking);
        }

        public async Task<TicketDto?> GetTicketAsync(int bookingId, string userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking is null) return null;

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to view this ticket.");

            var profile = await _userClient.GetUserProfileAsync(userId);
            var user = await _userClient.GetUserAsync(userId);
            var flight = await _flightClient.GetFlightDetailsAsync(booking.FlightId);

            return new TicketDto
            {
                BookingReference = booking.BookingReference,
                BookingStatus = booking.BookingStatus.ToString(),
                CreatedAt = booking.CreatedAt,
                TotalAmount = booking.TotalAmount,

                PassengerName = profile?.FullName ?? (user != null ? $"{user.FirstName} {user.LastName}" : "N/A"),
                Email = profile?.Email ?? user?.Email ?? "N/A",

                FlightNumber = flight?.FlightNumber ?? "N/A",
                Origin = flight?.Origin ?? "N/A",
                Destination = flight?.Destination ?? "N/A",
                DepartureTime = flight?.DepartureTime ?? default,
                ArrivalTime = flight?.ArrivalTime ?? default,
                FlightStatus = flight?.Status ?? "N/A",

                Passengers = booking.Passengers.Select(p => new PassengerDto
                {
                    FullName = p.FullName,
                    Age = p.Age,
                    Gender = p.Gender.ToString(),
                    PassengerType = p.PassengerType.ToString(),
                    PassportNumber = p.PassportNumber,
                    SeatNumber = p.SeatNumber
                }).ToList()
            };
        }

        public async Task<List<BookingDetailsDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            var result = new List<BookingDetailsDto>();
            
            foreach (var booking in bookings)
            {
                var dto = await MapToDetailsWithFlightInfoAsync(booking);
                result.Add(dto);
            }
            
            return result;
        }

        public async Task<List<BookingDetailsDto>> GetBookingsByFlightIdAsync(int flightId)
        {
            var bookings = await _bookingRepository.GetByFlightIdAsync(flightId);
            return bookings.Select(MapToDetails).ToList();
        }

        private static string GenerateBookingReference() =>
            $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        private static BookingResponseDto MapToResponse(Booking booking) => new()
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,
            BookingStatus = booking.BookingStatus.ToString(),
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt
        };

        private static BookingDetailsDto MapToDetails(Booking booking) => new()
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,
            UserId = booking.UserId,
            FlightId = booking.FlightId,
            ReturnFlightId = booking.ReturnFlightId,
            BookingType = booking.BookingType.ToString(),
            BookingStatus = booking.BookingStatus.ToString(),
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt,
            Passengers = booking.Passengers.Select(p => new PassengerDto
            {
                FullName = p.FullName,
                Age = p.Age,
                Gender = p.Gender.ToString(),
                PassengerType = p.PassengerType.ToString(),
                PassportNumber = p.PassportNumber,
                SeatNumber = p.SeatNumber
            }).ToList()
        };
        
        private async Task<BookingDetailsDto> MapToDetailsWithFlightInfoAsync(Booking booking)
        {
            FlightServiceClient.FlightDto? flight = null;
            
            try
            {
                flight = await _flightClient.GetFlightDetailsAsync(booking.FlightId);
            }
            catch (Exception)
            {
                // Log error but continue with null flight data
            }
            
            return new BookingDetailsDto
            {
                Id = booking.Id,
                BookingReference = booking.BookingReference,
                UserId = booking.UserId,
                FlightId = booking.FlightId,
                ReturnFlightId = booking.ReturnFlightId,
                BookingType = booking.BookingType.ToString(),
                BookingStatus = booking.BookingStatus.ToString(),
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt,
                FlightNumber = flight?.FlightNumber ?? "N/A",
                Origin = flight?.Origin ?? "N/A",
                Destination = flight?.Destination ?? "N/A",
                DepartureTime = flight?.DepartureTime,
                ArrivalTime = flight?.ArrivalTime,
                FlightStatus = flight?.Status ?? "N/A",
                Passengers = booking.Passengers.Select(p => new PassengerDto
                {
                    FullName = p.FullName,
                    Age = p.Age,
                    Gender = p.Gender.ToString(),
                    PassengerType = p.PassengerType.ToString(),
                    PassportNumber = p.PassportNumber,
                    SeatNumber = p.SeatNumber
                }).ToList()
            };
        }
    }
}
