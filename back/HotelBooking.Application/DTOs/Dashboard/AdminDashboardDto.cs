namespace HotelBooking.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    AdminRevenueDto Revenue,
    AdminBookingStatsDto BookingStats,
    // Every hotel with its performance for the selected period (zero-activity hotels included),
    // ordered by gross revenue. The client sorts by whatever column the admin clicks.
    List<HotelRankingDto> Hotels
);

public record AdminRevenueDto(
    decimal TotalPlatformRevenue,      // مجموع 15% من كل الحجوزات
    decimal TotalCancellationRevenue,  // الـ 20% من الإلغاءات تذهب للمنصة
    decimal TotalRevenue               // الإجمالي
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
