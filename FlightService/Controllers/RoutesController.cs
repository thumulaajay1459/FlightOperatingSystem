using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlightService.Data;
using FlightService.DTOs;
using FlightRoute = FlightService.Models.Route;
using Microsoft.AspNetCore.Authorization;

namespace FlightService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly appDataContext _context;

        public RoutesController(appDataContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddRoute([FromBody] CreateRouteDto dto)
        {
            var route = new FlightRoute
            {
                OriginAirportId = dto.OriginAirportId,
                DestinationAirportId = dto.DestinationAirportId,
                DistanceKm = dto.DistanceKm
            };
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(new { route.RouteId, route.OriginAirportId, route.DestinationAirportId, route.DistanceKm });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool paginate = true)
        {
            if (!paginate)
            {
                // Return all data without pagination for backward compatibility
                var allRoutes = await _context.Routes
                    .Include(r => r.Origin)
                    .Include(r => r.Destination)
                    .ToListAsync();
                return Ok(allRoutes.Select(r => new 
                { 
                    r.RouteId, 
                    r.OriginAirportId, 
                    r.DestinationAirportId, 
                    r.DistanceKm,
                    OriginAirport = r.Origin != null ? new { r.Origin.Code, r.Origin.Name, r.Origin.City } : null,
                    DestinationAirport = r.Destination != null ? new { r.Destination.Code, r.Destination.Name, r.Destination.City } : null
                }));
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = await _context.Routes.CountAsync();
            var routes = await _context.Routes
                .Include(r => r.Origin)
                .Include(r => r.Destination)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = routes.Select(r => new 
                { 
                    r.RouteId, 
                    r.OriginAirportId, 
                    r.DestinationAirportId, 
                    r.DistanceKm,
                    OriginAirport = r.Origin != null ? new { r.Origin.Code, r.Origin.Name, r.Origin.City } : null,
                    DestinationAirport = r.Destination != null ? new { r.Destination.Code, r.Destination.Name, r.Destination.City } : null
                }),
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
        public async Task<IActionResult> GetRoute(int id)
        {
            var route = await _context.Routes
                .Include(r => r.Origin)
                .Include(r => r.Destination)
                .FirstOrDefaultAsync(r => r.RouteId == id);
            if (route == null) return NotFound();
            return Ok(new 
            { 
                route.RouteId, 
                route.OriginAirportId, 
                route.DestinationAirportId, 
                route.DistanceKm,
                Origin = route.Origin != null ? new { route.Origin.Code, route.Origin.Name, route.Origin.City } : null,
                Destination = route.Destination != null ? new { route.Destination.Code, route.Destination.Name, route.Destination.City } : null
            });
        }

        // PUT api/Routes/{id} - Update route
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDto dto)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            route.OriginAirportId = dto.OriginAirportId;
            route.DestinationAirportId = dto.DestinationAirportId;
            route.DistanceKm = dto.DistanceKm;

            await _context.SaveChangesAsync();
            return Ok(new { route.RouteId, route.OriginAirportId, route.DestinationAirportId, route.DistanceKm });
        }



        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();
            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
