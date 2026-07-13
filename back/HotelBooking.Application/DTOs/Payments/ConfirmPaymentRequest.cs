namespace HotelBooking.Application.DTOs.Payments;

public record ConfirmPaymentRequest(
    long PaymentId,
    string TransactionRef
);
