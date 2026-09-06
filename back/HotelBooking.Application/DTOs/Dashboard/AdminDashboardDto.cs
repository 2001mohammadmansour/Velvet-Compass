namespace HotelBooking.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    AdminRevenueDto Revenue,
    AdminBookingStatsDto BookingStats,
    // Every hotel with its performance for the selected period (zero-activity hotels included),
    // ordered by gross revenue. The client sorts by whatever column the admin clicks.
    List<HotelRankingDto> Hotels
);

public record AdminRevenueDto(
    // The 15% commission accrued on every confirmed/completed booking.
    decimal TotalPlatformRevenue,
    // The platform's 15% cut of cancellation penalties. The penalty money stays in the owner's
    // wallet, so this is owed by owners (it shows in the Commission tab's "pending" total) — NOT
    // collected platform revenue. Surfaced here only so the admin sees it accruing.
    decimal CancellationCommissionOwed,
    // = TotalPlatformRevenue. The cancellation cut is intentionally excluded (it's owed, not revenue).
    decimal TotalRevenue
);

public record AdminBookingStatsDto(
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    int CompletedBookings,
    int PendingBookings,
    int TotalHotels,
    int TotalUsers
);

public record HotelRankingDto(
    long HotelId,
    string HotelName,
    string City,
    string Country,
    int StarRating,
    decimal GrossRevenue,
    decimal PlatformRevenue,
    int BookingsCount,
    int CancelledCount
);
