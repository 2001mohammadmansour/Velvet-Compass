using HotelBooking.Application.DTOs.Dashboard;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
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

        var paidBookings = bookings.Where(b =>
            b.Status == BookingStatus.Confirmed ||
            b.Status == BookingStatus.Completed).ToList();

        var cancelledBookings = bookings.Where(b =>
            b.Status == BookingStatus.Cancelled ||
            b.Status == BookingStatus.NoShow).ToList();

        // ─── Platform Revenue ─────────────────────────────────
        var platformRevenue = paidBookings.Sum(b => b.PlatformFee);
        // The platform's 15% share of cancellation penalties (the owner keeps the other 85%).
        var cancellationRevenue = Math.Round(cancelledBookings.Sum(b => b.CancellationPenalty ?? 0) * 0.15m, 2);
        var totalPlatformRevenue = platformRevenue + cancellationRevenue;

        // ─── Realisation split + settlement position ──────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        bool Matured(Booking b) => b.SettledAt != null || b.CheckoutDate < today;
        bool DueUnsettled(Booking b) => b.SettledAt == null && b.CheckoutDate < today;

        var earnedPlatformRevenue = paidBookings.Where(Matured).Sum(b => b.PlatformFee);
        var pendingPlatformRevenue = platformRevenue - earnedPlatformRevenue;

        var owedToOwners = paidBookings
            .Where(b => b.PaymentMethod == PaymentMethod.Online && DueUnsettled(b))
            .Sum(b => b.OwnerAmount);
        var owedByOwners = paidBookings
            .Where(b => b.PaymentMethod == PaymentMethod.CashOnArrival && DueUnsettled(b))
            .Sum(b => b.PlatformFee);

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
            new AdminRevenueDto(
                platformRevenue, cancellationRevenue, totalPlatformRevenue,
                earnedPlatformRevenue, pendingPlatformRevenue,
                owedToOwners, owedByOwners),
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
            .Where(b => b.CheckinDate >= from && b.CheckinDate <= to)
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
