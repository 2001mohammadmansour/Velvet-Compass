namespace HotelBooking.Application.DTOs.Bookings;

public record BookingDto(
    long Id,
    long HotelId,
    string HotelName,
    string Status,
    DateOnly CheckinDate,
    DateOnly CheckoutDate,
    int TotalNights,
    decimal TotalAmount,
    string? SpecialRequests,
    DateTime CreatedAt,
    List<BookingItemDto> Items,
    List<GuestDto> Guests,
    PaymentSummaryDto? Payment,
    // CHANGED BY AI (2026-07-12): please review. Exposes the breakfast add-on on a booking.
    bool IncludeBreakfast,
    decimal BreakfastFee,
    // CHANGED BY AI (2026-07-13): please review. New fields for the Reviews feature — default to
    // false here since MapToDto (a plain sync mapper) can't check the Reviews table; GetByIdAsync
    // fills in the real values afterward via a `with` expression.
    bool Reviewable = false,
    bool HasReview = false,
    // CHANGED BY AI (2026-07-13): please review. Exposes the hotel's real cancellation policy on
    // the booking itself, so the guest UI's "is this cancellation free?" check is accurate
    // instead of assuming a hardcoded default. CancellationFeeType is "Percentage" or "Flat".
    bool FreeCancellationEnabled = true,
    int FreeCancellationDaysBefore = 0,
    string CancellationFeeType = "Percentage",
    decimal CancellationFeeValue = 0m,
    // Set when this booking's dates were modified within the late-fee window (see
    // BookingService.ModifyDatesAsync); null if never modified or modified for free.
    decimal? ModificationFee = null
);

public record BookingItemDto(
    long Id,
    string RoomTypeName,
    int Qty,
    int Nights,
    decimal PricePerNight,
    decimal TotalPrice,
    // CHANGED BY AI (2026-07-15): please review. Extra-bed count/fee for this line, fixed at
    // booking time (see BookingService.CreateAsync/ModifyDatesAsync).
    int ExtraBedCount = 0,
    decimal ExtraBedFee = 0m
);

public record GuestDto(
    long Id,
    string FullName,
    string? PassportNo,
    string? Nationality,
    bool IsPrimary
);

public record PaymentSummaryDto(
    long Id,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    DateTime? PaidAt
);
