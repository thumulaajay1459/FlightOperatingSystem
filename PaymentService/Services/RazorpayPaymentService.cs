using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Interfaces;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using PaymentModel = PaymentService.Models.Payment;
using PaymentStatus = PaymentService.Models.PaymentStatus;

namespace PaymentService.Services;

public class RazorpayPaymentService(AppDbContext db, IConfiguration config) : IPaymentService
{
    private readonly string _keyId = config["Razorpay:KeyId"]!;
    private readonly string _keySecret = config["Razorpay:KeySecret"]!;

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            var total = request.BaseFare + request.TaxAmount + request.ServiceFee;

            var client = new RazorpayClient(_keyId, _keySecret);
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(total * 100) },
                { "currency", request.Currency },
                { "receipt", request.BookingId },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);
            string razorpayOrderId = order["id"].ToString();

            var payment = new PaymentModel
            {
                BookingId = request.BookingId,
                FlightId = request.FlightId,
                FlightNumber = request.FlightNumber,
                Origin = request.Origin,
                Destination = request.Destination,
                TravelDate = request.TravelDate,
                PassengerId = request.PassengerId,
                PassengerName = request.PassengerName,
                Email = request.Email,
                Phone = request.Phone,
                SeatNumber = request.SeatNumber,
                CabinClass = request.CabinClass,
                BaseFare = request.BaseFare,
                TaxAmount = request.TaxAmount,
                ServiceFee = request.ServiceFee,
                TotalAmount = total,
                Currency = request.Currency,
                RazorpayOrderId = razorpayOrderId,
                Status = PaymentStatus.OrderCreated
            };

            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            return new CreateOrderResponse
            {
                PaymentId = payment.PaymentId,
                RazorpayOrderId = razorpayOrderId,
                TotalAmount = total,
                Currency = request.Currency,
                Key = _keyId
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Razorpay order: {ex.Message}", ex);
        }
    }

    public async Task<PaymentResponse> VerifyPaymentAsync(VerifyPaymentRequest request)
    {
        var payment = await db.Payments.FindAsync(request.PaymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status == PaymentStatus.Success)
            throw new InvalidOperationException("Payment already verified.");

        var payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
        var expectedSignature = ComputeHmacSha256(payload, _keySecret);

        if (expectedSignature != request.RazorpaySignature)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Signature verification failed.";
            payment.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid payment signature.");
        }

        payment.RazorpayPaymentId = request.RazorpayPaymentId;
        payment.RazorpaySignature = request.RazorpaySignature;
        payment.Status = PaymentStatus.Success;
        payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Confirm booking after successful payment
        await ConfirmBookingAsync(payment.BookingId);

        return MapToResponse(payment);
    }
    
    private async Task ConfirmBookingAsync(string bookingId)
    {
        try
        {
            var bookingServiceUrl = config["Services:BookingService"] ?? "http://localhost:5004";
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"{bookingServiceUrl}/api/booking/{bookingId}/confirm-payment", 
                null);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to confirm booking {bookingId}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming booking {bookingId}: {ex.Message}");
        }
    }

    private async Task FailBookingAsync(string bookingId)
    {
        try
        {
            var bookingServiceUrl = config["Services:BookingService"] ?? "http://localhost:5004";
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"{bookingServiceUrl}/api/booking/{bookingId}/fail-payment", 
                null);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to mark booking {bookingId} as failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking booking {bookingId} as failed: {ex.Message}");
        }
    }

    public async Task<PaymentResponse?> GetByPaymentIdAsync(Guid paymentId)
    {
        var payment = await db.Payments.FindAsync(paymentId);
        return payment is null ? null : MapToResponse(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetByBookingIdAsync(string bookingId)
    {
        var payments = await db.Payments
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return payments.Select(MapToResponse);
    }

    public async Task<PaymentResponse> RefundAsync(RefundRequest request)
    {
        var payment = await db.Payments.FindAsync(request.PaymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Success)
            throw new InvalidOperationException("Only successful payments can be refunded.");

        var client = new RazorpayClient(_keyId, _keySecret);
        var refundOptions = new Dictionary<string, object>
        {
            { "amount", (int)(payment.TotalAmount * 100) },
            { "notes", new Dictionary<string, string> { { "reason", request.Reason } } }
        };

        var razorpayPayment = client.Payment.Fetch(payment.RazorpayPaymentId!);
        razorpayPayment.Refund(refundOptions);

        payment.Status = PaymentStatus.Refunded;
        payment.FailureReason = request.Reason;
        payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync()
    {
        var payments = await db.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return payments.Select(MapToResponse);
    }

    public async Task<IEnumerable<PaymentResponse>> GetUserPaymentsAsync(string userId)
    {
        // Note: Payment model doesn't have UserId, using PassengerId as proxy
        var payments = await db.Payments
            .Where(p => p.PassengerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return payments.Select(MapToResponse);
    }

    public async Task<PaymentResponse> FailPaymentAsync(FailPaymentRequest request)
    {
        var payment = await db.Payments.FindAsync(request.PaymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (payment.Status == PaymentStatus.Success)
            throw new InvalidOperationException("Cannot fail a successful payment.");

        if (payment.Status == PaymentStatus.Failed)
            return MapToResponse(payment);

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = request.Reason;
        payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Mark booking as failed - NO notification sent
        await FailBookingAsync(payment.BookingId);

        return MapToResponse(payment);
    }

    public async Task ExpirePendingPaymentsAsync()
    {
        var expiryMinutes = int.Parse(config["Payment:ExpiryMinutes"] ?? "15");
        var cutoffTime = DateTime.UtcNow.AddMinutes(-expiryMinutes);

        var expiredPayments = await db.Payments
            .Where(p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.OrderCreated) 
                     && p.CreatedAt < cutoffTime)
            .ToListAsync();

        foreach (var payment in expiredPayments)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Payment expired - User did not complete payment";
            payment.UpdatedAt = DateTime.UtcNow;
            
            // Mark booking as failed - NO notification sent
            await FailBookingAsync(payment.BookingId);
        }

        if (expiredPayments.Any())
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"Expired {expiredPayments.Count} pending payments");
        }
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static PaymentResponse MapToResponse(PaymentModel p) => new()
    {
        PaymentId = p.PaymentId,
        BookingId = p.BookingId,
        FlightNumber = p.FlightNumber,
        Origin = p.Origin,
        Destination = p.Destination,
        TravelDate = p.TravelDate,
        PassengerName = p.PassengerName,
        SeatNumber = p.SeatNumber,
        CabinClass = p.CabinClass,
        BaseFare = p.BaseFare,
        TaxAmount = p.TaxAmount,
        ServiceFee = p.ServiceFee,
        TotalAmount = p.TotalAmount,
        Currency = p.Currency,
        Status = p.Status.ToString(),
        RazorpayPaymentId = p.RazorpayPaymentId,
        CreatedAt = p.CreatedAt
    };
}
