using HotelBooking.Application.DTOs.Dashboard;

namespace HotelBooking.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<DateRangeRevenueDto> GetPlatformRevenueByDateRangeAsync(DateOnly from, DateOnly to);
}
