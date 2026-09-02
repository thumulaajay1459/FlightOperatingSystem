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
    public class AirportsController : ControllerBase
    {
        private readonly appDataContext _context;

        public AirportsController(appDataContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddAirport([FromBody] CreateAirportDto dto)
        {
            var airport = new Airport
            {
                Code = dto.Code,
                Name = dto.Name,
                City = dto.City,
                Country = dto.Country
            };
            _context.Airports.Add(airport);
            await _context.SaveChangesAsync();
            return Ok(new { airport.AirportId, airport.Code, airport.Name, airport.City, airport.Country });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAirports([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool paginate = true)
        {
            if (!paginate)
            {
                // Return all data without pagination for backward compatibility
                var allAirports = await _context.Airports.ToListAsync();
                return Ok(allAirports.Select(a => new { a.AirportId, a.Code, a.Name, a.City, a.Country }));
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = await _context.Airports.CountAsync();
            var airports = await _context.Airports
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = airports.Select(a => new { a.AirportId, a.Code, a.Name, a.City, a.Country }),
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        [HttpGet]
        [Route("/api/Airports/public")]
        public async Task<IActionResult> GetPublicAirports([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool paginate = true)
        {
            if (!paginate)
            {
                // Return all data without pagination for backward compatibility
                var allAirports = await _context.Airports.ToListAsync();
                return Ok(allAirports.Select(a => new { a.AirportId, a.Code, a.Name, a.City, a.Country }));
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = await _context.Airports.CountAsync();
            var airports = await _context.Airports
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = airports.Select(a => new { a.AirportId, a.Code, a.Name, a.City, a.Country }),
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
        public async Task<IActionResult> GetAirport(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null) return NotFound();
            return Ok(new { airport.AirportId, airport.Code, airport.Name, airport.City, airport.Country });
        }

        // PUT api/Airports/{id} - Update airport
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, [FromBody] UpdateAirportDto dto)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null) return NotFound();

            airport.Code = dto.Code;
            airport.Name = dto.Name;
            airport.City = dto.City;
            airport.Country = dto.Country;

            await _context.SaveChangesAsync();
            return Ok(new { airport.AirportId, airport.Code, airport.Name, airport.City, airport.Country });
        }

        // DELETE api/Airports/{id} - Delete airport
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null) return NotFound();

            _context.Airports.Remove(airport);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
