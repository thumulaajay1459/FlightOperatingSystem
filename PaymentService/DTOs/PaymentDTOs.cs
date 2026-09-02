using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs;

public class CreateOrderRequest
{
    [Required] public string BookingId { get; set; } = string.Empty;
    [Required] public string FlightId { get; set; } = string.Empty;
    [Required] public string FlightNumber { get; set; } = string.Empty;
    [Required] public string Origin { get; set; } = string.Empty;
    [Required] public string Destination { get; set; } = string.Empty;
    [Required] public DateTime TravelDate { get; set; }

    [Required] public string PassengerId { get; set; } = string.Empty;
    [Required] public string PassengerName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;

    [Required] public string SeatNumber { get; set; } = string.Empty;
    [Required] public string CabinClass { get; set; } = string.Empty;

    [Required, Range(1, double.MaxValue)] public decimal BaseFare { get; set; }
    [Required, Range(0, double.MaxValue)] public decimal TaxAmount { get; set; }
    [Required, Range(0, double.MaxValue)] public decimal ServiceFee { get; set; }
    public string Currency { get; set; } = "INR";
}

public class CreateOrderResponse
{
    public Guid PaymentId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty; // Razorpay Key ID for frontend
}

public class VerifyPaymentRequest
{
    [Required] public Guid PaymentId { get; set; }
    [Required] public string RazorpayOrderId { get; set; } = string.Empty;
    [Required] public string RazorpayPaymentId { get; set; } = string.Empty;
    [Required] public string RazorpaySignature { get; set; } = string.Empty;
}

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime TravelDate { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string CabinClass { get; set; } = string.Empty;
    public decimal BaseFare { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RazorpayPaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RefundRequest
{
    [Required] public Guid PaymentId { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
}

public class FailPaymentRequest
{
    [Required] public Guid PaymentId { get; set; }
    public string Reason { get; set; } = "User exited without payment";
}


