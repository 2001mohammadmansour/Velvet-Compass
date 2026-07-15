namespace HotelBooking.Application.DTOs.Bookings;

public record BookingSummaryDto(
    long Id,
    string HotelName,
    string Status,
    DateOnly CheckinDate,
    DateOnly CheckoutDate,
    decimal TotalAmount,
    DateTime CreatedAt,
    // CHANGED BY AI (2026-07-13): please review. New fields for the Reviews feature — used by the
    // admin user-detail drill-down (previously showed a permanent "not available yet" placeholder).
    bool HasReview = false,
    decimal? ReviewScore = null,
    string? ReviewComment = null,
    // CHANGED BY AI (2026-07-13): please review. Lets the admin panel fetch the full review
    // detail / delete it (GET|DELETE /api/v1/reviews/{reviewId}).
    long? ReviewId = null,
    // CHANGED BY AI (2026-07-15): please review. Platform's cut of this booking. Only populated
    // by BookingService.GetHotelBookingsAsync when the caller is an Admin (0 otherwise, e.g. for
    // a guest's own GetMyBookingsAsync) — used by the admin Revenue Stats tab, which used to sum
    // gross totalAmount and was reporting ~6x the platform's actual earnings.
    decimal PlatformFee = 0m
);
