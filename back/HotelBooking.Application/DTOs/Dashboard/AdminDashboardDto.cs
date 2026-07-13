namespace HotelBooking.Application.DTOs.Dashboard;

public record AdminDashboardDto(
    AdminRevenueDto Revenue,
    AdminBookingStatsDto BookingStats,
    List<HotelRankingDto> TopHotelsByRevenue,
    List<HotelRankingDto> TopHotelsByBookings
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
