using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models;

public class Payment
{
    [Key]
    public Guid PaymentId { get; set; } = Guid.NewGuid();

    // Booking & Flight Info
    public string BookingId { get; set; } = string.Empty;
    public string FlightId { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }

    // Passenger Info
    public string PassengerId { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Seat & Class
    public string SeatNumber { get; set; } = string.Empty;
    public string CabinClass { get; set; } = string.Empty; // Economy, Business, First

    // Amount Details
    public decimal BaseFare { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";

    // Razorpay
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }

    // Status & Audit
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum PaymentStatus
{
    Pending,
    OrderCreated,
    Success,
    Failed,
    Refunded
}
