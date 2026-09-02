using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingService.Data;
using BookingService.Models;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(BookingDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET api/admin/dashboard/stats - Dashboard statistics
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalBookings = await _context.Bookings.CountAsync();
            var totalRevenue = await _context.Bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed)
                .SumAsync(b => b.TotalAmount);

            // Get unique user count and flight count from bookings
            var totalUsers = await _context.Bookings.Select(b => b.UserId).Distinct().CountAsync();
            var totalFlights = await _context.Bookings.Select(b => b.FlightId).Distinct().CountAsync();

            return Ok(new
            {
                totalUsers,
                totalFlights,
                totalBookings,
                totalRevenue
            });
        }

        // GET api/admin/reports/revenue - Revenue report
        [HttpGet("reports/revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow;

            var bookings = await _context.Bookings
                .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
                .Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed)
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            var bookingCount = bookings.Count;
            var averageBookingValue = bookingCount > 0 ? totalRevenue / bookingCount : 0;

            var dailyRevenue = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new
                {
                    date = g.Key,
                    revenue = g.Sum(b => b.TotalAmount),
                    bookings = g.Count()
                })
                .OrderBy(x => x.date)
                .ToList();

            return Ok(new
            {
                fromDate,
                toDate,
                totalRevenue,
                bookingCount,
                averageBookingValue,
                dailyRevenue
            });
        }

        // GET api/admin/reports/popular-routes - Popular routes report
        [HttpGet("reports/popular-routes")]
        public async Task<IActionResult> GetPopularRoutes([FromQuery] int top = 10)
        {
            var bookingsByFlight = await _context.Bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed)
                .GroupBy(b => b.FlightId)
                .Select(g => new
                {
                    flightId = g.Key,
                    totalBookings = g.Count(),
                    totalRevenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.totalBookings)
                .Take(top)
                .ToListAsync();

            return Ok(bookingsByFlight);
        }
    }
}
