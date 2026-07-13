namespace HotelBooking.Application.DTOs.Payments;

public record PaymentDto(
    long Id,
    long BookingId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? TransactionRef,
    DateTime? PaidAt,
    DateTime CreatedAt
);
