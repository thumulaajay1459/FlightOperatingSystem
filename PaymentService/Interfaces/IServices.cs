using PaymentService.DTOs;

namespace PaymentService.Interfaces;

public interface IPaymentService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<PaymentResponse> VerifyPaymentAsync(VerifyPaymentRequest request);
    Task<PaymentResponse?> GetByPaymentIdAsync(Guid paymentId);
    Task<IEnumerable<PaymentResponse>> GetByBookingIdAsync(string bookingId);
    Task<PaymentResponse> RefundAsync(RefundRequest request);
    Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync();
    Task<IEnumerable<PaymentResponse>> GetUserPaymentsAsync(string userId);
    Task<PaymentResponse> FailPaymentAsync(FailPaymentRequest request);
    Task ExpirePendingPaymentsAsync();
}


