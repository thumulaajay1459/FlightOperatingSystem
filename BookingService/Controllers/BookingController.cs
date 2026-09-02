using System.Security.Claims;
using BookingService.DTOs;
using BookingService.Interfaces;
using BookingService.Models;
using BookingService.Repositories;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly HttpClient _notificationClient;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingService bookingService,
            IBookingRepository bookingRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _notificationClient = httpClientFactory.CreateClient("NotificationService");
            _logger = logger;
        }

        // POST api/booking/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            var userId = GetUserId();
            var booking = await _bookingService.CreateBookingAsync(userId, request);
            
            // Don't send notification yet - wait for payment confirmation
            // Notification will be sent after payment verification
            
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, booking);
        }

        // GET api/booking/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var userId = GetUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id, userId);
            return booking is null ? NotFound() : Ok(booking);
        }

        // GET api/booking/my-bookings
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool paginate = true)
        {
            var userId = GetUserId();
            var allBookings = await _bookingService.GetBookingsByUserIdAsync(userId);

            if (!paginate)
            {
                // Return all data without pagination for backward compatibility
                return Ok(allBookings);
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = allBookings.Count();
            var paginatedBookings = allBookings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                data = paginatedBookings,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // GET api/booking/{id}/ticket
        [HttpGet("{id:int}/ticket")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var userId = GetUserId();
            var ticket = await _bookingService.GetTicketAsync(id, userId);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        // GET api/booking/{id}/download-ticket
        [HttpGet("{id:int}/download-ticket")]
        public async Task<IActionResult> DownloadTicket(int id)
        {
            var userId = GetUserId();
            var ticket = await _bookingService.GetTicketAsync(id, userId);
            
            if (ticket is null) return NotFound();

            try
            {
                var pdfBytes = FlightTicketPdfGenerator.Generate(ticket);
                return File(pdfBytes, "application/pdf", $"FlightTicket_{ticket.BookingReference}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate PDF for booking {BookingId}", id);
                return StatusCode(500, "Failed to generate PDF ticket");
            }
        }

        // POST api/booking/{id}/confirm-payment - Confirm booking after payment
        [HttpPost("{id:int}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var userId = GetUserId();
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null) return NotFound();
            if (booking.UserId != userId && !User.IsInRole("Admin")) return Forbid();
            
            if (booking.BookingStatus == BookingStatus.Confirmed)
            {
                return Ok(new { message = "Booking already confirmed." });
            }

            if (booking.BookingStatus == BookingStatus.Cancelled)
            {
                return BadRequest("Cannot confirm cancelled booking.");
            }

            // Update status to Confirmed
            booking.BookingStatus = BookingStatus.Confirmed;
            await _bookingRepository.SaveChangesAsync();

            // Send notification with PDF only after successful payment confirmation
            _ = SendBookingNotificationWithPdfAsync(booking.Id, userId);

            return Ok(new { message = "Booking confirmed successfully.", bookingId = booking.Id });
        }

        // POST api/booking/{id}/fail-payment - Mark booking as failed when payment fails
        [HttpPost("{id:int}/fail-payment")]
        public async Task<IActionResult> FailPayment(int id)
        {
            var userId = GetUserId();
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null) return NotFound();
            if (booking.UserId != userId && !User.IsInRole("Admin")) return Forbid();
            
            if (booking.BookingStatus == BookingStatus.Confirmed)
            {
                return BadRequest("Cannot fail confirmed booking.");
            }

            if (booking.BookingStatus == BookingStatus.Failed)
            {
                return Ok(new { message = "Booking already marked as failed." });
            }

            // Update status to Failed - NO notification sent
            booking.BookingStatus = BookingStatus.Failed;
            await _bookingRepository.SaveChangesAsync();

            _logger.LogInformation("Booking {BookingId} marked as failed due to payment failure", booking.Id);

            return Ok(new { message = "Booking marked as failed.", bookingId = booking.Id });
        }

        // POST api/booking/cancel?id={id}
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelBooking([FromQuery] int id)
        {
            var userId = GetUserId();
            var booking = await _bookingService.CancelBookingAsync(id, userId);
            return Ok(booking);
        }

        // PUT api/booking/{id} - Update booking passengers
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDto request)
        {
            var userId = GetUserId();
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null) return NotFound();
            if (booking.UserId != userId && !User.IsInRole("Admin")) return Forbid();
            if (booking.BookingStatus == Models.BookingStatus.Cancelled) return BadRequest("Cannot update cancelled booking.");

            booking.Passengers.Clear();
            foreach (var p in request.Passengers)
            {
                booking.Passengers.Add(new Models.Passenger
                {
                    FullName = p.FullName,
                    Age = p.Age,
                    Gender = Enum.TryParse<Models.Gender>(p.Gender, true, out var g) ? g : Models.Gender.Male,
                    PassengerType = Enum.TryParse<Models.PassengerType>(p.PassengerType, true, out var pt) ? pt : Models.PassengerType.Adult,
                    PassportNumber = p.PassportNumber ?? string.Empty,
                    SeatNumber = p.SeatNumber
                });
            }

            await _bookingRepository.SaveChangesAsync();
            return Ok(new { message = "Booking updated successfully." });
        }

        // GET api/booking/all - Get all bookings (Admin only)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var allBookings = await _bookingService.GetAllBookingsAsync();
            var totalCount = allBookings.Count();
            var paginatedBookings = allBookings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                data = paginatedBookings,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // GET api/booking/by-flight/{flightId} - Get bookings by flight (Admin only)
        [HttpGet("by-flight/{flightId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookingsByFlight(int flightId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var allBookings = await _bookingService.GetBookingsByFlightIdAsync(flightId);
            var totalCount = allBookings.Count();
            var paginatedBookings = allBookings.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                data = paginatedBookings,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // POST api/booking/{id}/resend-ticket - Resend ticket email
        [HttpPost("{id:int}/resend-ticket")]
        public async Task<IActionResult> ResendTicket(int id)
        {
            var userId = GetUserId();
            var ticket = await _bookingService.GetTicketAsync(id, userId);
            
            if (ticket == null) return NotFound();

            try
            {
                // Generate PDF
                var pdfBytes = FlightTicketPdfGenerator.Generate(ticket);

                // Send to NotificationService with PDF
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(ticket.Email), "recipientEmail");
                content.Add(new StringContent(ticket.PassengerName), "recipientName");
                content.Add(new StringContent(ticket.BookingReference), "bookingId");
                content.Add(new StringContent(ticket.FlightNumber), "flightNumber");
                content.Add(new ByteArrayContent(pdfBytes), "pdfFile", $"FlightTicket_{ticket.BookingReference}.pdf");

                await _notificationClient.PostAsync("/api/notifications/send-ticket-with-pdf", content);
                _logger.LogInformation("Ticket resent for booking {BookingId}", id);
                
                return Ok(new { message = "Ticket resent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend ticket for booking {BookingId}", id);
                return StatusCode(500, "Failed to resend ticket.");
            }
        }

        private async Task SendBookingNotificationWithPdfAsync(int bookingId, string userId)
        {
            try
            {
                var ticket = await _bookingService.GetTicketAsync(bookingId, userId);
                if (ticket == null) return;

                // Generate PDF
                var pdfBytes = FlightTicketPdfGenerator.Generate(ticket);

                // Send to NotificationService with PDF
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(ticket.Email), "recipientEmail");
                content.Add(new StringContent(ticket.PassengerName), "recipientName");
                content.Add(new StringContent(ticket.BookingReference), "bookingId");
                content.Add(new StringContent(ticket.FlightNumber), "flightNumber");
                content.Add(new ByteArrayContent(pdfBytes), "pdfFile", $"FlightTicket_{ticket.BookingReference}.pdf");

                await _notificationClient.PostAsync("/api/notifications/send-ticket-with-pdf", content);
                _logger.LogInformation("Notification with PDF sent for booking {BookingId}", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification with PDF for booking {BookingId}", bookingId);
            }
        }

        private async Task SendBookingNotificationAsync(int bookingId, string userId)
        {
            try
            {
                var ticket = await _bookingService.GetTicketAsync(bookingId, userId);
                if (ticket == null) return;

                var notification = new
                {
                    RecipientEmail = ticket.Email,
                    RecipientName = ticket.PassengerName,
                    BookingId = ticket.BookingReference,
                    FlightNumber = ticket.FlightNumber,
                    DepartureCity = ticket.Origin,
                    ArrivalCity = ticket.Destination,
                    DepartureTime = ticket.DepartureTime,
                    ArrivalTime = ticket.ArrivalTime,
                    SeatNumber = ticket.Passengers.FirstOrDefault()?.SeatNumber ?? "N/A",
                    TotalPrice = ticket.TotalAmount
                };

                await _notificationClient.PostAsJsonAsync("/api/notifications/ticket-confirmation", notification);
                _logger.LogInformation("Notification sent for booking {BookingId}", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for booking {BookingId}", bookingId);
            }
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User identity could not be resolved from token.");

            return userId;
        }
    }
}
