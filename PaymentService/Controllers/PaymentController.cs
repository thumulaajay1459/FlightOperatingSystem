using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Interfaces;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await paymentService.CreateOrderAsync(request);
        return Ok(result);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        var result = await paymentService.VerifyPaymentAsync(request);
        return Ok(result);
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<IActionResult> GetByPaymentId(Guid paymentId)
    {
        var result = await paymentService.GetByPaymentIdAsync(paymentId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetByBookingId(string bookingId)
    {
        var result = await paymentService.GetByBookingIdAsync(bookingId);
        return Ok(result);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        var result = await paymentService.RefundAsync(request);
        return Ok(result);
    }

    // GET api/payments/all - Get all payments (Admin only)
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await paymentService.GetAllPaymentsAsync();
        var totalCount = result.Count();
        
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var paginatedResult = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            data = paginatedResult,
            pagination = new
            {
                currentPage = page,
                pageSize = pageSize,
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    // GET api/payments/user/{userId} - Get user payment history
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserPayments(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await paymentService.GetUserPaymentsAsync(userId);
        var totalCount = result.Count();
        
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var paginatedResult = result.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            data = paginatedResult,
            pagination = new
            {
                currentPage = page,
                pageSize = pageSize,
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    // POST api/payments/webhook - Razorpay webhook handler
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook([FromBody] object webhookData)
    {
        // TODO: Implement Razorpay webhook signature verification
        // For now, return OK to acknowledge receipt
        return Ok(new { message = "Webhook received" });
    }

    // POST api/payments/fail - Mark payment as failed
    [HttpPost("fail")]
    public async Task<IActionResult> FailPayment([FromBody] FailPaymentRequest request)
    {
        var result = await paymentService.FailPaymentAsync(request);
        return Ok(result);
    }
}
