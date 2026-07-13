using HotelBooking.Application.DTOs.Payments;

namespace HotelBooking.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> InitiateAsync(long userId, InitiatePaymentRequest request);
    Task<PaymentDto> ConfirmAsync(long userId, ConfirmPaymentRequest request);
    Task<PaymentDto> RefundAsync(long callerId, bool isAdmin, long paymentId);
    Task<PaymentDto> GetByBookingIdAsync(long callerId, bool isAdmin, long bookingId);
}
