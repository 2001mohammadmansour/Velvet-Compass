namespace HotelBooking.Application.DTOs.Dashboard;

public record DateRangeRevenueRequest(
    long HotelId,
    DateOnly From,
    DateOnly To
);

public record DateRangeRevenueDto(
    DateOnly From,
    DateOnly To,
    decimal GrossRevenue,
    decimal PlatformFee,
    decimal NetRevenue,
    int BookingsCount,
    int CancelledCount
);
