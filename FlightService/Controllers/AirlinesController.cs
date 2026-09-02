using FlightService.Data;
using FlightService.DTO;
using FlightService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirlinesController : ControllerBase
    {
        private readonly appDataContext _context;

        public AirlinesController(appDataContext context)
        {
            _context = context;
        }

        // GET: api/Airlines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAirlines()
        {
            var airlines = await _context.Airlines
                .Select(a => new AirlineDto
                {
                    AirlineId = a.AirlineId,
                    Code = a.Code,
                    Name = a.Name,
                    Country = a.Country,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return Ok(airlines);
        }

        // GET: api/Airlines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AirlineDto>> GetAirline(int id)
        {
            var airline = await _context.Airlines.FindAsync(id);

            if (airline == null)
                return NotFound();

            return Ok(new AirlineDto
            {
                AirlineId = airline.AirlineId,
                Code = airline.Code,
                Name = airline.Name,
                Country = airline.Country,
                IsActive = airline.IsActive
            });
        }

        // POST: api/Airlines
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AirlineDto>> CreateAirline(CreateAirlineDto dto)
        {
            if (await _context.Airlines.AnyAsync(a => a.Code == dto.Code))
                return BadRequest($"Airline with code {dto.Code} already exists.");

            var airline = new Airline
            {
                Code = dto.Code.ToUpper(),
                Name = dto.Name,
                Country = dto.Country,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Airlines.Add(airline);
            await _context.SaveChangesAsync();

            var result = new AirlineDto
            {
                AirlineId = airline.AirlineId,
                Code = airline.Code,
                Name = airline.Name,
                Country = airline.Country,
                IsActive = airline.IsActive
            };

            return CreatedAtAction(nameof(GetAirline), new { id = airline.AirlineId }, result);
        }

        // PUT: api/Airlines/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAirline(int id, UpdateAirlineDto dto)
        {
            var airline = await _context.Airlines.FindAsync(id);

            if (airline == null)
                return NotFound();

            airline.Name = dto.Name;
            airline.Country = dto.Country;
            airline.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Airlines/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAirline(int id)
        {
            var airline = await _context.Airlines.FindAsync(id);

            if (airline == null)
                return NotFound();

            _context.Airlines.Remove(airline);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
