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

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Hotel)
            .ToListAsync();

        var paidBookings = bookings.Where(b =>
            b.Status == BookingStatus.Confirmed ||
            b.Status == BookingStatus.Completed).ToList();

        var cancelledBookings = bookings.Where(b =>
            b.Status == BookingStatus.Cancelled).ToList();

        // ─── Platform Revenue ─────────────────────────────────
        var platformRevenue = paidBookings.Sum(b => b.PlatformFee);
        var cancellationRevenue = cancelledBookings.Sum(b => b.CancellationPenalty ?? 0);
        var totalPlatformRevenue = platformRevenue + cancellationRevenue;

        // ─── Top Hotels By Revenue ────────────────────────────
        var topByRevenue = bookings
            .Where(b => b.Status == BookingStatus.Confirmed ||
                        b.Status == BookingStatus.Completed)
            .GroupBy(b => new
            {
                b.HotelId,
                b.Hotel.Name,
                b.Hotel.City,
                b.Hotel.Country,
                b.Hotel.StarRating
            })
            .Select(g => new HotelRankingDto(
                g.Key.HotelId,
                g.Key.Name,
                g.Key.City,
                g.Key.Country,
                g.Key.StarRating,
                g.Sum(b => b.TotalAmount),
                g.Sum(b => b.PlatformFee),
                g.Count(),
                bookings.Count(b => b.HotelId == g.Key.HotelId &&
                                    b.Status == BookingStatus.Cancelled)
            ))
            .OrderByDescending(h => h.GrossRevenue)
            .Take(10)
            .ToList();

        // ─── Top Hotels By Bookings ───────────────────────────
        var topByBookings = bookings
            .GroupBy(b => new
            {
                b.HotelId,
                b.Hotel.Name,
                b.Hotel.City,
                b.Hotel.Country,
                b.Hotel.StarRating
            })
            .Select(g => new HotelRankingDto(
                g.Key.HotelId,
                g.Key.Name,
                g.Key.City,
                g.Key.Country,
                g.Key.StarRating,
                g.Where(b => b.Status == BookingStatus.Confirmed ||
                             b.Status == BookingStatus.Completed)
                 .Sum(b => b.TotalAmount),
                g.Where(b => b.Status == BookingStatus.Confirmed ||
                             b.Status == BookingStatus.Completed)
                 .Sum(b => b.PlatformFee),
                g.Count(),
                g.Count(b => b.Status == BookingStatus.Cancelled)
            ))
            .OrderByDescending(h => h.BookingsCount)
            .Take(10)
            .ToList();

        var totalHotels = await _context.Hotels.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        return new AdminDashboardDto(
            new AdminRevenueDto(platformRevenue, cancellationRevenue, totalPlatformRevenue),
            new AdminBookingStatsDto(
                bookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Confirmed),
                cancelledBookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Completed),
                bookings.Count(b => b.Status == BookingStatus.Pending),
                totalHotels,
                totalUsers
            ),
            topByRevenue,
            topByBookings
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
