using HotelBooking.Application.DTOs.Dashboard;

namespace HotelBooking.Application.Interfaces;

public interface IOwnerDashboardService
{
    Task<OwnerDashboardDto> GetDashboardAsync(long callerId, bool isAdmin, long hotelId);
    Task<DateRangeRevenueDto> GetRevenueByDateRangeAsync(long callerId, bool isAdmin, DateRangeRevenueRequest request);
    Task<List<RoomPerformanceDto>> GetRoomPerformanceAsync(long callerId, bool isAdmin, long hotelId, DateOnly? from, DateOnly? to);
    Task<List<CalendarDayDto>> GetCalendarAsync(long callerId, bool isAdmin, long hotelId, DateOnly from, DateOnly to);
    Task TrackViewAsync(long hotelId);
    Task TrackClickAsync(long hotelId);
}
