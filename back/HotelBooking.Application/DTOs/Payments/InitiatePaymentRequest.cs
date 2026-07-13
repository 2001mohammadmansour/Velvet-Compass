namespace HotelBooking.Application.DTOs.Payments;

public record InitiatePaymentRequest(
    long BookingId,
    string Method,
    string Currency = "USD"
);
