namespace HotelBooking.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    AdminRevenueDto Revenue,
    AdminBookingStatsDto BookingStats,
    // Every hotel with its performance for the selected period (zero-activity hotels included),
    // ordered by gross revenue. The client sorts by whatever column the admin clicks.
    List<HotelRankingDto> Hotels
);

public record AdminRevenueDto(
    // Money the platform has actually received. Today = commission owners have paid and the admin
    // has confirmed; a distinct figure so other income sources can be folded in later.
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
