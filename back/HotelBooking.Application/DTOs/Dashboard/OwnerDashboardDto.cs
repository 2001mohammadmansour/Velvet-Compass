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
    decimal GrossRevenue,       // قبل النسبة — total booked value (in-play bookings)
    decimal PlatformFee,        // 15% للمنصة
    decimal NetRevenue,         // 85% للمالك
    decimal CancellationLosses, // kept for back-compat: total penalty on cancelled bookings

    // ── Owner money position (the "wallet") ──────────────────────────────
    // From ONLINE bookings — the platform collected the cash and owes the owner 85%:
    decimal AvailableToOwner,     // stay finished, not yet settled → owed to the owner now
    decimal PendingToOwner,       // stay not finished yet → will be owed
    decimal PaidOutToOwner,       // already settled → lifetime received from the platform

    // From CASH bookings — the owner collected the cash and owes the platform 15%:
    decimal CommissionDue,        // stay finished, not yet settled → owner owes this now
    decimal CommissionPending,    // stay not finished yet
    decimal CommissionPaid,       // already settled → lifetime commission paid

    decimal CashInHand,           // owner's 85% from matured cash bookings — already collected
    decimal PenaltyIncome,        // owner's 85% share of cancellation penalties
    decimal NetPositionNow        // AvailableToOwner - CommissionDue
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
