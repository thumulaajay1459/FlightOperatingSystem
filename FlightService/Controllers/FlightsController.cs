using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightService.Data;
using FlightService.Models;
using FlightService.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/ScheduledFlights")]
    public class ScheduledFlightsController : ControllerBase
    {
        private readonly appDataContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ScheduledFlightsController> _logger;

        public ScheduledFlightsController(appDataContext context, IHttpClientFactory httpClientFactory, ILogger<ScheduledFlightsController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> AddFlight([FromBody] CreateFlightDto dto)
        {
            var flight = new Flight
            {
                FlightNumber = dto.FlightNumber,
                AircraftId = dto.AircraftId,
                RouteId = dto.RouteId,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                Status = dto.Status,
                BasePrice = dto.BasePrice,
                EconomyPrice = dto.EconomyPrice,
                PremiumEconomyPrice = dto.PremiumEconomyPrice,
                BusinessPrice = dto.BusinessPrice,
                FirstClassPrice = dto.FirstClassPrice,
                ChildDiscountPercent = dto.ChildDiscountPercent,
                InfantDiscountPercent = dto.InfantDiscountPercent
            };
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFlightById), new { id = flight.FlightId }, ToDto(flight));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        //[Consumes("application/json")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] UpdateFlightDto dto)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return NotFound();
            
            var oldStatus = flight.Status;
            var oldDepartureTime = flight.DepartureTime;
            
            flight.FlightNumber = dto.FlightNumber;
            flight.AircraftId = dto.AircraftId;
            flight.RouteId = dto.RouteId;
            flight.DepartureTime = dto.DepartureTime;
            flight.ArrivalTime = dto.ArrivalTime;
            flight.Status = dto.Status;
            flight.Comments = dto.Comments;
            flight.DelayedMinutes = dto.DelayedMinutes;
            
            if (dto.DelayedMinutes.HasValue && dto.DelayedMinutes.Value > 0)
            {
                flight.ActualDepartureTime = flight.DepartureTime.AddMinutes(dto.DelayedMinutes.Value);
                flight.ActualArrivalTime = flight.ArrivalTime.AddMinutes(dto.DelayedMinutes.Value);
            }
            
            await _context.SaveChangesAsync();
            
            // Send notifications if status changed to Delayed or Cancelled
            if ((dto.Status == "Delayed" && oldStatus != "Delayed") || 
                (dto.Status == "Cancelled" && oldStatus != "Cancelled"))
            {
                _ = SendFlightStatusNotification(id, dto.Status, dto.Comments, dto.DelayedMinutes);
            }
            
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return NotFound();
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFlights([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var totalCount = await _context.Flights.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var flights = await _context.Flights
                    .Include(f => f.Aircraft)
                    .Include(f => f.Route)
                        .ThenInclude(r => r!.Origin)
                    .Include(f => f.Route)
                        .ThenInclude(r => r!.Destination)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    data = flights.Select(ToSummaryDto),
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllFlights: {Message}", ex.Message);
                return StatusCode(500, new { error = "Failed to retrieve flights", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Origin)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Destination)
                .FirstOrDefaultAsync(f => f.FlightId == id);
            if (flight == null) return NotFound();
            return Ok(ToSummaryDto(flight));
        }

        // User-friendly search: Search flights from A to B with date, filters, and round trip support
        [HttpGet("search-flights")]
        public async Task<IActionResult> SearchFlightsByLocation(
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] DateTime? date,
            [FromQuery] DateTime? returnDate,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? timeOfDay,
            [FromQuery] string? sortBy = "price")
        {
            // Validation
            if (date.HasValue && date.Value.Date < DateTime.UtcNow.Date)
                return BadRequest(new { error = "Departure date cannot be in the past" });

            if (returnDate.HasValue && date.HasValue && returnDate.Value.Date < date.Value.Date)
                return BadRequest(new { error = "Return date must be after departure date" });

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to) && from.Equals(to, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Origin and destination cannot be the same" });

            // Outbound flights
            var outboundFlights = await SearchFlightsInternal(from, to, date, minPrice, maxPrice, timeOfDay);

            // Return flights (if round trip)
            List<FlightSearchDto>? returnFlights = null;
            if (returnDate.HasValue && !string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                returnFlights = await SearchFlightsInternal(to, from, returnDate, minPrice, maxPrice, timeOfDay);
            }

            // Sort results
            outboundFlights = SortFlights(outboundFlights, sortBy);
            if (returnFlights != null)
                returnFlights = SortFlights(returnFlights, sortBy);

            return Ok(new
            {
                searchCriteria = new
                {
                    from = from ?? "any",
                    to = to ?? "any",
                    departureDate = date?.Date,
                    returnDate = returnDate?.Date,
                    tripType = returnDate.HasValue ? "RoundTrip" : "OneWay",
                    priceRange = minPrice.HasValue || maxPrice.HasValue ? new { min = minPrice, max = maxPrice } : null,
                    timeOfDay = timeOfDay,
                    sortBy = sortBy
                },
                outbound = new
                {
                    totalFlights = outboundFlights.Count,
                    flights = outboundFlights
                },
                returnTrip = returnFlights != null ? new
                {
                    totalFlights = returnFlights.Count,
                    flights = returnFlights
                } : null
            });
        }

        private async Task<List<FlightSearchDto>> SearchFlightsInternal(
            string? from, string? to, DateTime? date,
            decimal? minPrice, decimal? maxPrice, string? timeOfDay)
        {
            var query = _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Origin)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Destination)
                .Where(f => f.Status == "Scheduled")
                .AsNoTracking();

            // Location filters
            if (!string.IsNullOrEmpty(from))
            {
                var fromUpper = from.ToUpper();
                query = query.Where(f => f.Route!.Origin!.City.ToUpper().Contains(fromUpper) ||
                                        f.Route.Origin.Code.ToUpper().Contains(fromUpper));
            }

            if (!string.IsNullOrEmpty(to))
            {
                var toUpper = to.ToUpper();
                query = query.Where(f => f.Route!.Destination!.City.ToUpper().Contains(toUpper) ||
                                        f.Route.Destination.Code.ToUpper().Contains(toUpper));
            }

            // Date filter
            if (date.HasValue)
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);
            else
                query = query.Where(f => f.DepartureTime >= DateTime.UtcNow);

            // Time of day filter
            if (!string.IsNullOrEmpty(timeOfDay))
            {
                query = timeOfDay.ToLower() switch
                {
                    "morning" => query.Where(f => f.DepartureTime.Hour >= 6 && f.DepartureTime.Hour < 12),
                    "afternoon" => query.Where(f => f.DepartureTime.Hour >= 12 && f.DepartureTime.Hour < 18),
                    "evening" => query.Where(f => f.DepartureTime.Hour >= 18 && f.DepartureTime.Hour < 24),
                    "night" => query.Where(f => f.DepartureTime.Hour >= 0 && f.DepartureTime.Hour < 6),
                    _ => query
                };
            }

            var flights = await query.OrderBy(f => f.DepartureTime).ToListAsync();

            // Get seat availability for each flight
            var flightIds = flights.Select(f => f.FlightId).ToList();
            var bookedSeatsDict = await _context.Set<FlightSeat>()
                .Where(fs => flightIds.Contains(fs.FlightId) &&
                      (fs.Status == SeatStatus.Booked ||
                       (fs.Status == SeatStatus.Blocked && fs.BlockedUntil > DateTime.UtcNow)))
                .GroupBy(fs => fs.FlightId)
                .Select(g => new { FlightId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FlightId, x => x.Count);

            var results = flights.Select(f =>
            {
                var totalSeats = f.Aircraft?.TotalSeats ?? 0;
                var bookedSeats = bookedSeatsDict.GetValueOrDefault(f.FlightId, 0);
                var availableSeats = totalSeats - bookedSeats;

                return new FlightSearchDto
                {
                    FlightId = f.FlightId,
                    FlightNumber = f.FlightNumber,
                    Origin = f.Route?.Origin?.City ?? "",
                    OriginCode = f.Route?.Origin?.Code ?? "",
                    Destination = f.Route?.Destination?.City ?? "",
                    DestinationCode = f.Route?.Destination?.Code ?? "",
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    DurationHours = Math.Round((f.ArrivalTime - f.DepartureTime).TotalHours, 2),
                    Status = f.Status,
                    Aircraft = $"{f.Aircraft?.Manufacturer} {f.Aircraft?.Model}",
                    TotalSeats = totalSeats,
                    AvailableSeats = availableSeats,
                    EconomyPrice = f.EconomyPrice,
                    PremiumEconomyPrice = f.PremiumEconomyPrice,
                    BusinessPrice = f.BusinessPrice,
                    FirstClassPrice = f.FirstClassPrice,
                    ChildDiscountPercent = f.ChildDiscountPercent,
                    InfantDiscountPercent = f.InfantDiscountPercent
                };
            }).ToList();

            // Price filter
            if (minPrice.HasValue)
                results = results.Where(f => f.EconomyPrice >= minPrice.Value).ToList();
            if (maxPrice.HasValue)
                results = results.Where(f => f.EconomyPrice <= maxPrice.Value).ToList();

            return results;
        }

        private static List<FlightSearchDto> SortFlights(List<FlightSearchDto> flights, string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "price" => flights.OrderBy(f => f.EconomyPrice).ToList(),
                "duration" => flights.OrderBy(f => f.DurationHours).ToList(),
                "departure" => flights.OrderBy(f => f.DepartureTime).ToList(),
                "arrival" => flights.OrderBy(f => f.ArrivalTime).ToList(),
                _ => flights.OrderBy(f => f.EconomyPrice).ToList()
            };
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(
            [FromQuery] string? flightNumber, 
            [FromQuery] DateTime? departure, 
            [FromQuery] string? status,
            [FromQuery] string? origin,
            [FromQuery] string? destination,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Origin)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Destination)
                .Where(f => f.DepartureTime >= DateTime.UtcNow)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(flightNumber))
            {
                var flightNumUpper = flightNumber.ToUpper();
                query = query.Where(f => f.FlightNumber.ToUpper().Contains(flightNumUpper));
            }

            if (departure.HasValue)
                query = query.Where(f => f.DepartureTime.Date == departure.Value.Date);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);

            if (!string.IsNullOrEmpty(origin))
            {
                var originUpper = origin.ToUpper();
                query = query.Where(f => f.Route!.Origin!.City.ToUpper().Contains(originUpper) || 
                                        f.Route.Origin.Code.ToUpper().Contains(originUpper));
            }

            if (!string.IsNullOrEmpty(destination))
            {
                var destUpper = destination.ToUpper();
                query = query.Where(f => f.Route!.Destination!.City.ToUpper().Contains(destUpper) || 
                                        f.Route.Destination.Code.ToUpper().Contains(destUpper));
            }

            var totalCount = await query.CountAsync();
            var flights = await query
                .OrderBy(f => f.DepartureTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            var flightIds = flights.Select(f => f.FlightId).ToList();
            var bookedSeatsDict = await _context.Set<FlightSeat>()
                .Where(fs => flightIds.Contains(fs.FlightId) &&
                      (fs.Status == SeatStatus.Booked ||
                       (fs.Status == SeatStatus.Blocked && fs.BlockedUntil > DateTime.UtcNow)))
                .GroupBy(fs => fs.FlightId)
                .Select(g => new { FlightId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FlightId, x => x.Count);

            var results = flights.Select(f =>
            {
                var totalSeats = f.Aircraft?.TotalSeats ?? 0;
                var bookedSeats = bookedSeatsDict.GetValueOrDefault(f.FlightId, 0);
                var availableSeats = totalSeats - bookedSeats;

                return new FlightSearchDto
                {
                    FlightId = f.FlightId,
                    FlightNumber = f.FlightNumber,
                    Origin = f.Route?.Origin?.City ?? "",
                    OriginCode = f.Route?.Origin?.Code ?? "",
                    Destination = f.Route?.Destination?.City ?? "",
                    DestinationCode = f.Route?.Destination?.Code ?? "",
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    DurationHours = Math.Round((f.ArrivalTime - f.DepartureTime).TotalHours, 2),
                    Status = f.Status,
                    Aircraft = $"{f.Aircraft?.Manufacturer} {f.Aircraft?.Model}",
                    TotalSeats = totalSeats,
                    AvailableSeats = availableSeats,
                    EconomyPrice = f.EconomyPrice,
                    PremiumEconomyPrice = f.PremiumEconomyPrice,
                    BusinessPrice = f.BusinessPrice,
                    FirstClassPrice = f.FirstClassPrice,
                    ChildDiscountPercent = f.ChildDiscountPercent,
                    InfantDiscountPercent = f.InfantDiscountPercent
                };
            }).ToList();

            return Ok(new
            {
                data = results,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // GET api/ScheduledFlights/available-seats/{id} - Get available seats for a flight
        [HttpGet("available-seats/{id}")]
        public async Task<IActionResult> GetAvailableSeats(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.FlightId == id);

            if (flight == null) return NotFound();

            var totalSeats = flight.Aircraft?.TotalSeats ?? 0;
            
            // Count booked and blocked seats from FlightSeat table
            var bookedSeats = await _context.Set<FlightSeat>()
                .Where(fs => fs.FlightId == id && 
                      (fs.Status == SeatStatus.Booked || 
                       (fs.Status == SeatStatus.Blocked && fs.BlockedUntil > DateTime.UtcNow)))
                .CountAsync();
            
            var availableSeats = totalSeats - bookedSeats;

            return Ok(new { flightId = id, totalSeats, bookedSeats, availableSeats });
        }

        // GET api/ScheduledFlights/by-route - Get flights by route
        [HttpGet("by-route")]
        public async Task<IActionResult> GetFlightsByRoute(
            [FromQuery] int originId, 
            [FromQuery] int destinationId, 
            [FromQuery] DateTime? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Origin)
                .Include(f => f.Route)
                    .ThenInclude(r => r!.Destination)
                .Where(f => f.Route!.OriginAirportId == originId && f.Route.DestinationAirportId == destinationId)
                .AsNoTracking();

            if (date.HasValue)
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);

            var totalCount = await query.CountAsync();
            var flights = await query
                .OrderBy(f => f.DepartureTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = flights.Select(ToSummaryDto),
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // PUT api/ScheduledFlights/{id}/pricing - Update flight pricing (Admin)
        [HttpPut("{id}/pricing")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateFlightPricing(int id, [FromBody] UpdateFlightPricingDto dto)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return NotFound();

            flight.BasePrice = dto.BasePrice;
            flight.EconomyPrice = dto.EconomyPrice;
            flight.PremiumEconomyPrice = dto.PremiumEconomyPrice;
            flight.BusinessPrice = dto.BusinessPrice;
            flight.FirstClassPrice = dto.FirstClassPrice;
            flight.ChildDiscountPercent = dto.ChildDiscountPercent;
            flight.InfantDiscountPercent = dto.InfantDiscountPercent;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Pricing updated successfully", flight });
        }

        // PUT api/ScheduledFlights/bulk-update-pricing - Bulk update all flights pricing (Admin)
        [HttpPut("bulk-update-pricing")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        public async Task<IActionResult> BulkUpdatePricing([FromBody] UpdateFlightPricingDto dto)
        {
            var flights = await _context.Flights.ToListAsync();
            
            foreach (var flight in flights)
            {
                flight.BasePrice = dto.BasePrice;
                flight.EconomyPrice = dto.EconomyPrice;
                flight.PremiumEconomyPrice = dto.PremiumEconomyPrice;
                flight.BusinessPrice = dto.BusinessPrice;
                flight.FirstClassPrice = dto.FirstClassPrice;
                flight.ChildDiscountPercent = dto.ChildDiscountPercent;
                flight.InfantDiscountPercent = dto.InfantDiscountPercent;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Pricing updated for {flights.Count} flights", updatedCount = flights.Count });
        }

        // POST api/ScheduledFlights/calculate-price - Calculate price for booking
        [HttpPost("calculate-price")]
        [Consumes("application/json")]
        public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequestDto request)
        {
            var flight = await _context.Flights.FindAsync(request.FlightId);
            if (flight == null) return NotFound("Flight not found");

            Flight? returnFlight = null;
            if (request.ReturnFlightId.HasValue)
            {
                returnFlight = await _context.Flights.FindAsync(request.ReturnFlightId.Value);
                if (returnFlight == null) return NotFound("Return flight not found");
            }

            var outboundPrice = CalculateFlightPrice(flight, request.Passengers);
            var returnPrice = returnFlight != null ? CalculateFlightPrice(returnFlight, request.Passengers) : ((decimal, decimal, decimal, List<object>)?)null;

            var breakdown = new
            {
                outbound = outboundPrice,
                returnTrip = returnPrice,
                totalAmount = outboundPrice.Item3 + (returnPrice?.Item3 ?? 0)
            };

            return Ok(breakdown);
        }

        private static FlightSummaryDto ToSummaryDto(Flight f) => new()
        {
            FlightId = f.FlightId,
            FlightNumber = f.FlightNumber,
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            Status = f.Status,
            Manufacturer = f.Aircraft?.Manufacturer ?? string.Empty,
            Model = f.Aircraft?.Model ?? string.Empty,
            TotalSeats = f.Aircraft?.TotalSeats ?? 0,
            Origin = f.Route?.Origin != null ? $"{f.Route.Origin.City} ({f.Route.Origin.Code})" : string.Empty,
            Destination = f.Route?.Destination != null ? $"{f.Route.Destination.City} ({f.Route.Destination.Code})" : string.Empty,
            DistanceKm = f.Route?.DistanceKm ?? 0
        };

        private static FlightDto ToDto(Flight f) => new()
        {
            FlightId = f.FlightId,
            FlightNumber = f.FlightNumber,
            AircraftId = f.AircraftId,
            RouteId = f.RouteId,
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            Status = f.Status
        };

        private (decimal baseFare, decimal taxes, decimal totalAmount, List<object> passengerPrices) CalculateFlightPrice(
            Flight flight, List<PassengerPricingDto> passengers)
        {
            var passengerPrices = new List<object>();
            decimal totalBaseFare = 0;

            foreach (var passenger in passengers)
            {
                decimal basePrice = passenger.SeatClass switch
                {
                    "Economy" => flight.EconomyPrice,
                    "PremiumEconomy" => flight.PremiumEconomyPrice,
                    "Business" => flight.BusinessPrice,
                    "FirstClass" => flight.FirstClassPrice,
                    _ => flight.EconomyPrice
                };

                decimal discount = passenger.PassengerType switch
                {
                    "Child" => basePrice * (flight.ChildDiscountPercent / 100),
                    "Infant" => basePrice * (flight.InfantDiscountPercent / 100),
                    _ => 0
                };

                decimal finalPrice = basePrice - discount;
                totalBaseFare += finalPrice;

                passengerPrices.Add(new
                {
                    passengerType = passenger.PassengerType,
                    seatClass = passenger.SeatClass,
                    seatNumber = passenger.SeatNumber,
                    basePrice = basePrice,
                    discount = discount,
                    finalPrice = finalPrice
                });
            }

            decimal taxes = totalBaseFare * 0.18m; // 18% tax
            decimal totalAmount = totalBaseFare + taxes;

            return (totalBaseFare, taxes, totalAmount, passengerPrices);
        }
        
        private async Task SendFlightStatusNotification(int flightId, string status, string? comments, int? delayedMinutes)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://localhost:5005");
                
                var notification = new
                {
                    FlightId = flightId,
                    Status = status,
                    Comments = comments,
                    DelayedMinutes = delayedMinutes
                };
                
                await client.PostAsJsonAsync("/api/notifications/flight-status-change", notification);
                _logger.LogInformation($"Flight status notification sent for flight {flightId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send flight status notification for flight {flightId}");
            }
        }
    }

    public class PriceCalculationRequestDto
    {
        public int FlightId { get; set; }
        public int? ReturnFlightId { get; set; }
        public List<PassengerPricingDto> Passengers { get; set; } = [];
    }

    public class PassengerPricingDto
    {
        public string PassengerType { get; set; } = "Adult";
        public string SeatClass { get; set; } = "Economy";
        public string? SeatNumber { get; set; }
    }
}
