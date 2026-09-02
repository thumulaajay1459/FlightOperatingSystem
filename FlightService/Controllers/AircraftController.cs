using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightService.Data;
using FlightService.Models;
using FlightService.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AircraftController : ControllerBase
    {
        private readonly appDataContext _context;

        public AircraftController(appDataContext context) 
        {
             _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddAircraft([FromBody] CreateAircraftDto dto)
        {
            var aircraft = new Aircraft
            {
                Manufacturer = dto.Manufacturer,
                Model = dto.Model,
                TotalSeats = dto.TotalSeats,
            
            };
            _context.Aircrafts.Add(aircraft);
            await _context.SaveChangesAsync();
            return Ok(new { aircraft.AircraftId, aircraft.Manufacturer, aircraft.Model, aircraft.TotalSeats });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAircrafts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool paginate = true)
        {
            if (!paginate)
            {
                // Return all data without pagination for backward compatibility
                var allAircrafts = await _context.Aircrafts.ToListAsync();
                return Ok(allAircrafts.Select(a => new { a.AircraftId, a.Manufacturer, a.Model, a.TotalSeats }));
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = await _context.Aircrafts.CountAsync();
            var aircrafts = await _context.Aircrafts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = aircrafts.Select(a => new { a.AircraftId, a.Manufacturer, a.Model, a.TotalSeats }),
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAircraft(int id)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null) return NotFound();
            return Ok(new { aircraft.AircraftId, aircraft.Manufacturer, aircraft.Model, aircraft.TotalSeats });
        }

        // PUT api/Aircraft/{id} - Update aircraft
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAircraft(int id, [FromBody] UpdateAircraftDto dto)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null) return NotFound();

            aircraft.Manufacturer = dto.Manufacturer;
            aircraft.Model = dto.Model;
            aircraft.TotalSeats = dto.TotalSeats;

            await _context.SaveChangesAsync();
            return Ok(new { aircraft.AircraftId, aircraft.Manufacturer, aircraft.Model, aircraft.TotalSeats });
        }

        // DELETE api/Aircraft/{id} - Delete aircraft
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAircraft(int id)
        {
            var aircraft = await _context.Aircrafts.FindAsync(id);
            if (aircraft == null) return NotFound();

            _context.Aircrafts.Remove(aircraft);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
