namespace HotelBooking.Application.DTOs.Dashboard;

public record OwnerDashboardDto(
    long HotelId,
    string HotelName,
    RevenueDto Revenue,
    BookingStatsDto BookingStats,
    ViewStatsDto ViewStats,
    List<PeriodRevenueDto> MonthlyRevenue,
    List<PeriodRevenueDto> QuarterlyRevenue,
    List<PeriodRevenueDto> YearlyRevenue
);

public record RevenueDto(
    decimal GrossRevenue,       // قبل النسبة
    decimal PlatformFee,        // 15% للمنصة
    decimal NetRevenue,         // 85% للمالك
    decimal CancellationLosses  // الخسائر من الإلغاءات
);

public record BookingStatsDto(
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    int CompletedBookings,
    int PendingBookings
);

public record ViewStatsDto(
    int TotalViews,
    int TotalClicks,
    double ClickThroughRate  // Clicks / Views %
);

public record PeriodRevenueDto(
    string Period,          // "2025-01" أو "Q1 2025" أو "2025"
    decimal GrossRevenue,
    decimal NetRevenue,
    int BookingsCount
);
