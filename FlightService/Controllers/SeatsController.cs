using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightService.Data;
using FlightService.Models;
using FlightService.DTOs;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly appDataContext _context;

        public SeatsController(appDataContext context)
        {
            _context = context;
        }

        // GET api/Seats/flight/{flightId}/seat-map - Get seat map for a flight
        [HttpGet("flight/{flightId}/seat-map")]
        public async Task<IActionResult> GetSeatMap(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) return NotFound("Flight not found");
            if (flight.Aircraft == null) return NotFound("Aircraft not found for this flight");

            var seats = await _context.Set<Seat>()
                .Where(s => s.AircraftId == flight.AircraftId)
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Column)
                .ToListAsync();

            // If no seats exist, generate default seat map
            if (seats.Count == 0)
            {
                seats = GenerateDefaultSeats(flight.Aircraft);
            }

            var flightSeats = await _context.Set<FlightSeat>()
                .Where(fs => fs.FlightId == flightId)
                .Include(fs => fs.Seat)
                .ToListAsync();

            var seatMap = seats.Select(s =>
            {
                var flightSeat = flightSeats.FirstOrDefault(fs => fs.SeatId == s.SeatId);
                var status = flightSeat?.Status.ToString() ?? "Available";
                
                // Check if block expired
                if (status == "Blocked" && flightSeat?.BlockedUntil < DateTime.UtcNow)
                {
                    status = "Available";
                }

                return new
                {
                    seatId = s.SeatId,
                    seatNumber = s.SeatNumber,
                    row = s.Row,
                    column = s.Column,
                    seatClass = s.Class.ToString(),
                    status = status,
                    isWindowSeat = s.IsWindowSeat,
                    isAisleSeat = s.IsAisleSeat,
                    isExitRow = s.IsExitRow,
                    hasExtraLegroom = s.HasExtraLegroom,
                    extraCharge = s.ExtraCharge,
                    blockedUntil = flightSeat?.BlockedUntil
                };
            }).ToList();

            return Ok(seatMap);
        }

        private List<Seat> GenerateDefaultSeats(Aircraft aircraft)
        {
            var seats = new List<Seat>();
            var columns = new[] { "A", "B", "C", "D", "E", "F" };
            var totalRows = (aircraft.TotalSeats / columns.Length) + 1;
            
            // Calculate rows per class (approximate distribution)
            int firstClassRows = Math.Max(1, totalRows / 10);      // 10% First Class
            int businessRows = Math.Max(2, totalRows / 5);         // 20% Business
            int premiumEconomyRows = Math.Max(2, totalRows / 10);  // 10% Premium Economy
            int economyRows = totalRows - firstClassRows - businessRows - premiumEconomyRows;

            int currentRow = 1;

            // Create First Class seats (Rows 1-2)
            for (int i = 0; i < firstClassRows; i++)
            {
                foreach (var column in columns)
                {
                    if (seats.Count >= aircraft.TotalSeats) break;
                    seats.Add(new Seat
                    {
                        SeatId = 0,
                        AircraftId = aircraft.AircraftId,
                        SeatNumber = $"{currentRow}{column}",
                        Row = currentRow,
                        Column = column,
                        Class = SeatClass.FirstClass,
                        IsWindowSeat = column == "A" || column == "F",
                        IsAisleSeat = column == "C" || column == "D",
                        IsExitRow = false,
                        HasExtraLegroom = true,
                        ExtraCharge = 500
                    });
                }
                currentRow++;
                if (seats.Count >= aircraft.TotalSeats) break;
            }

            // Create Business Class seats
            for (int i = 0; i < businessRows; i++)
            {
                foreach (var column in columns)
                {
                    if (seats.Count >= aircraft.TotalSeats) break;
                    seats.Add(new Seat
                    {
                        SeatId = 0,
                        AircraftId = aircraft.AircraftId,
                        SeatNumber = $"{currentRow}{column}",
                        Row = currentRow,
                        Column = column,
                        Class = SeatClass.Business,
                        IsWindowSeat = column == "A" || column == "F",
                        IsAisleSeat = column == "C" || column == "D",
                        IsExitRow = false,
                        HasExtraLegroom = true,
                        ExtraCharge = 300
                    });
                }
                currentRow++;
                if (seats.Count >= aircraft.TotalSeats) break;
            }

            // Create Premium Economy seats
            for (int i = 0; i < premiumEconomyRows; i++)
            {
                foreach (var column in columns)
                {
                    if (seats.Count >= aircraft.TotalSeats) break;
                    seats.Add(new Seat
                    {
                        SeatId = 0,
                        AircraftId = aircraft.AircraftId,
                        SeatNumber = $"{currentRow}{column}",
                        Row = currentRow,
                        Column = column,
                        Class = SeatClass.PremiumEconomy,
                        IsWindowSeat = column == "A" || column == "F",
                        IsAisleSeat = column == "C" || column == "D",
                        IsExitRow = false,
                        HasExtraLegroom = false,
                        ExtraCharge = 100
                    });
                }
                currentRow++;
                if (seats.Count >= aircraft.TotalSeats) break;
            }

            // Create Economy Class seats
            for (int i = 0; i < economyRows; i++)
            {
                foreach (var column in columns)
                {
                    if (seats.Count >= aircraft.TotalSeats) break;
                    
                    bool isExitRow = (currentRow % 10 == 0); // Every 10th row is exit row
                    
                    seats.Add(new Seat
                    {
                        SeatId = 0,
                        AircraftId = aircraft.AircraftId,
                        SeatNumber = $"{currentRow}{column}",
                        Row = currentRow,
                        Column = column,
                        Class = SeatClass.Economy,
                        IsWindowSeat = column == "A" || column == "F",
                        IsAisleSeat = column == "C" || column == "D",
                        IsExitRow = isExitRow,
                        HasExtraLegroom = isExitRow,
                        ExtraCharge = isExitRow ? 50 : 0
                    });
                }
                currentRow++;
                if (seats.Count >= aircraft.TotalSeats) break;
            }

            return seats;
        }

        // POST api/Seats/block - Block seats temporarily
        [HttpPost("block")]
        [Authorize]
        public async Task<IActionResult> BlockSeats([FromBody] BlockSeatsRequestDto request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var seats = await _context.Set<Seat>()
                .Where(s => request.SeatNumbers.Contains(s.SeatNumber))
                .ToListAsync();

            if (seats.Count != request.SeatNumbers.Count)
                return BadRequest("Some seats not found");

            var flightSeats = await _context.Set<FlightSeat>()
                .Where(fs => fs.FlightId == request.FlightId && seats.Select(s => s.SeatId).Contains(fs.SeatId))
                .ToListAsync();

            // Check if any seat is already booked or blocked by someone else
            var unavailableSeats = flightSeats
                .Where(fs => fs.Status == SeatStatus.Booked || 
                            (fs.Status == SeatStatus.Blocked && fs.BlockedByUserId != userId && fs.BlockedUntil > DateTime.UtcNow))
                .ToList();

            if (unavailableSeats.Any())
                return BadRequest(new { message = "Some seats are not available", unavailableSeats = unavailableSeats.Select(fs => fs.Seat?.SeatNumber) });

            var blockUntil = DateTime.UtcNow.AddMinutes(request.BlockDurationMinutes);

            foreach (var seat in seats)
            {
                var flightSeat = flightSeats.FirstOrDefault(fs => fs.SeatId == seat.SeatId);
                
                if (flightSeat == null)
                {
                    // Create new flight seat entry
                    flightSeat = new FlightSeat
                    {
                        FlightId = request.FlightId,
                        SeatId = seat.SeatId,
                        Status = SeatStatus.Blocked,
                        BlockedByUserId = userId,
                        BlockedUntil = blockUntil
                    };
                    _context.Set<FlightSeat>().Add(flightSeat);
                }
                else
                {
                    // Update existing
                    flightSeat.Status = SeatStatus.Blocked;
                    flightSeat.BlockedByUserId = userId;
                    flightSeat.BlockedUntil = blockUntil;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seats blocked successfully",
                blockedSeats = request.SeatNumbers,
                blockedUntil = blockUntil
            });
        }

        // POST api/Seats/release - Release blocked seats
        [HttpPost("release")]
        [Authorize]
        public async Task<IActionResult> ReleaseSeats([FromBody] ReleaseSeatsRequestDto request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var seats = await _context.Set<Seat>()
                .Where(s => request.SeatNumbers.Contains(s.SeatNumber))
                .ToListAsync();

            var flightSeats = await _context.Set<FlightSeat>()
                .Where(fs => fs.FlightId == request.FlightId && 
                            seats.Select(s => s.SeatId).Contains(fs.SeatId) &&
                            fs.BlockedByUserId == userId)
                .ToListAsync();

            foreach (var flightSeat in flightSeats)
            {
                flightSeat.Status = SeatStatus.Available;
                flightSeat.BlockedByUserId = null;
                flightSeat.BlockedUntil = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Seats released successfully" });
        }

        // POST api/Seats/bulk-create - Create seats for aircraft (Admin)
        [HttpPost("bulk-create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCreateSeats([FromBody] BulkCreateSeatsDto request)
        {
            var aircraft = await _context.Aircrafts.FindAsync(request.AircraftId);
            if (aircraft == null) return NotFound("Aircraft not found");

            var seats = new List<Seat>();
            int currentRow = 1;

            // Create First Class seats
            for (int i = 0; i < request.FirstClassRows; i++)
            {
                foreach (var column in request.Columns)
                {
                    seats.Add(CreateSeat(request.AircraftId, currentRow, column, SeatClass.FirstClass, request));
                }
                currentRow++;
            }

            // Create Business Class seats
            for (int i = 0; i < request.BusinessRows; i++)
            {
                foreach (var column in request.Columns)
                {
                    seats.Add(CreateSeat(request.AircraftId, currentRow, column, SeatClass.Business, request));
                }
                currentRow++;
            }

            // Create Premium Economy seats
            for (int i = 0; i < request.PremiumEconomyRows; i++)
            {
                foreach (var column in request.Columns)
                {
                    seats.Add(CreateSeat(request.AircraftId, currentRow, column, SeatClass.PremiumEconomy, request));
                }
                currentRow++;
            }

            // Create Economy seats
            for (int i = 0; i < request.EconomyRows; i++)
            {
                foreach (var column in request.Columns)
                {
                    seats.Add(CreateSeat(request.AircraftId, currentRow, column, SeatClass.Economy, request));
                }
                currentRow++;
            }

            _context.Set<Seat>().AddRange(seats);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{seats.Count} seats created successfully", totalSeats = seats.Count });
        }

        private Seat CreateSeat(int aircraftId, int row, string column, SeatClass seatClass, BulkCreateSeatsDto request)
        {
            return new Seat
            {
                AircraftId = aircraftId,
                SeatNumber = $"{row}{column}",
                Row = row,
                Column = column,
                Class = seatClass,
                IsWindowSeat = request.WindowColumns.Contains(column),
                IsAisleSeat = request.AisleColumns.Contains(column),
                IsExitRow = request.ExitRows.Contains(row),
                HasExtraLegroom = request.ExitRows.Contains(row),
                ExtraCharge = seatClass switch
                {
                    SeatClass.FirstClass => 500,
                    SeatClass.Business => 300,
                    SeatClass.PremiumEconomy => 100,
                    _ => request.ExitRows.Contains(row) ? 50 : 0
                }
            };
        }
    }
}
