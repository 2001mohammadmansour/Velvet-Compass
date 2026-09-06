using HotelBooking.Application.DTOs.Dashboard;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Enum;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services.Dashboard;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context) => _context = context;

    public async Task<AdminDashboardDto> GetDashboardAsync(int? year = null, int? month = null)
    {
        DateOnly? from = null;
        DateOnly? to = null;
        if (year.HasValue && month.HasValue)
        {
            from = new DateOnly(year.Value, month.Value, 1);
            to = from.Value.AddMonths(1);
        }
        else if (year.HasValue)
        {
            from = new DateOnly(year.Value, 1, 1);
            to = from.Value.AddYears(1);
        }

        var bookings = await _context.Bookings
            .Include(b => b.Hotel)
            .Where(b => from == null || (b.CheckinDate >= from && b.CheckinDate < to))
            .ToListAsync();

        var cancelledBookings = bookings.Where(b =>
            b.Status == BookingStatus.Cancelled).ToList();

        // ─── Platform Revenue ─────────────────────────────────
        // Money the platform has actually received: commission an owner has paid and the admin
        // has confirmed. Everything else (accrued-but-unpaid, cancellation cuts) lives in the
        // Commission tab's own buckets. Kept a distinct figure so other income can be added here
        // later without touching the commission calc.
        var totalRevenue = bookings
            .Where(b => b.CommissionPaidAt != null)
            .Sum(b => b.CommissionAmount ?? 0m);

        // ─── Per-hotel performance (every hotel, zero-activity included) ──────
        var allHotels = await _context.Hotels.ToListAsync();
        var bookingsByHotel = bookings
            .GroupBy(b => b.HotelId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var hotelRows = allHotels
            .Select(h =>
            {
                var hb = bookingsByHotel.TryGetValue(h.Id, out var list) ? list : new List<Domain.Entities.Booking>();
                var paid = hb.Where(b => b.Status == BookingStatus.Confirmed ||
                                         b.Status == BookingStatus.Completed).ToList();
                return new HotelRankingDto(
                    h.Id,
                    h.Name,
                    h.City,
                    h.Country,
                    h.StarRating,
                    paid.Sum(b => b.TotalAmount),
                    paid.Sum(b => b.PlatformFee),
                    paid.Count,                                              // "bookings" = confirmed/completed
                    hb.Count(b => b.Status == BookingStatus.Cancelled));
            })
            .OrderByDescending(h => h.GrossRevenue)
            .ToList();

        var totalHotels = allHotels.Count;
        var totalUsers = await _context.Users.CountAsync();

        return new AdminDashboardDto(
            new AdminRevenueDto(totalRevenue),
            new AdminBookingStatsDto(
                bookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Confirmed),
                cancelledBookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Completed),
                bookings.Count(b => b.Status == BookingStatus.Pending),
                totalHotels,
                totalUsers
            ),
            hotelRows
        );
    }

    public async Task<DateRangeRevenueDto> GetPlatformRevenueByDateRangeAsync(
        DateOnly from, DateOnly to)
    {
        var bookings = await _context.Bookings
            .Where(b => DateOnly.FromDateTime(b.CreatedAt) >= from &&
                        DateOnly.FromDateTime(b.CreatedAt) <= to)
            .ToListAsync();

        var paidBookings = bookings.Where(b =>
            b.Status == BookingStatus.Confirmed ||
            b.Status == BookingStatus.Completed).ToList();

        return new DateRangeRevenueDto(
            from,
            to,
            paidBookings.Sum(b => b.TotalAmount),
            paidBookings.Sum(b => b.PlatformFee),
            paidBookings.Sum(b => b.OwnerAmount),
            paidBookings.Count,
            bookings.Count(b => b.Status == BookingStatus.Cancelled)
        );
    }
}
