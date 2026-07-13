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
    long? ReviewId = null
);
