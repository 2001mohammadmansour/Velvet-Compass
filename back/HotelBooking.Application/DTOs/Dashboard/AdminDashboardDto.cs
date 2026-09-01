namespace HotelBooking.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    AdminRevenueDto Revenue,
    AdminBookingStatsDto BookingStats,
    List<HotelRankingDto> TopHotelsByRevenue,
    List<HotelRankingDto> TopHotelsByBookings
);

public record AdminRevenueDto(
    decimal TotalPlatformRevenue,      // kept: 15% commission across all in-play bookings
    decimal TotalCancellationRevenue,  // kept: platform's 15% share of cancellation penalties
    decimal TotalRevenue,              // kept: the two combined

    // ── Realisation split ────────────────────────────────────────────────
    decimal EarnedPlatformRevenue,     // commission on bookings whose stay has finished
    decimal PendingPlatformRevenue,    // commission on bookings whose stay hasn't finished yet

    // ── Settlement position with owners ─────────────────────────────────
    decimal OwedToOwners,              // online, matured, unsettled — platform must pay owners
    decimal OwedByOwners               // cash, matured, unsettled — owners owe the platform
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
