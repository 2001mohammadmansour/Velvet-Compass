using HotelBooking.Application.DTOs.Dashboard;

namespace HotelBooking.Application.Interfaces;

public interface IAdminDashboardService
{
    // year/month null = all-time. year set, month null = that whole year. Both set = that month.
    // Scopes bookings by CheckinDate; TotalHotels/TotalUsers stay all-time registered counts
    // regardless of the filter.
    Task<AdminDashboardDto> GetDashboardAsync(int? year = null, int? month = null);
    Task<DateRangeRevenueDto> GetPlatformRevenueByDateRangeAsync(DateOnly from, DateOnly to);
}
