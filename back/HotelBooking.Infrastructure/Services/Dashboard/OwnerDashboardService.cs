using HotelBooking.Application.DTOs.Dashboard;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services.Dashboard;

public class OwnerDashboardService : IOwnerDashboardService
{
    private readonly AppDbContext _context;

    public OwnerDashboardService(AppDbContext context) => _context = context;

    public async Task<OwnerDashboardDto> GetDashboardAsync(long callerId, bool isAdmin, long hotelId)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
            ?? throw new HotelNotFoundException(hotelId);

        if (hotel.OwnerId != callerId && !isAdmin)
            throw new UnAuthoraizedOwnerException();

        var bookings = await _context.Bookings
            .Where(b => b.HotelId == hotelId)
            .ToListAsync();

        var paidBookings = bookings.Where(b =>
            b.Status == BookingStatus.Confirmed ||
            b.Status == BookingStatus.Completed).ToList();

        var cancelledBookings = bookings.Where(b =>
            b.Status == BookingStatus.Cancelled).ToList();

        // ─── Revenue ──────────────────────────────────────────
        var grossRevenue = paidBookings.Sum(b => b.TotalAmount);
        var platformFee = paidBookings.Sum(b => b.PlatformFee);
        var netRevenue = paidBookings.Sum(b => b.OwnerAmount);
        var cancelLosses = cancelledBookings.Sum(b => b.CancellationPenalty ?? 0);

        // ─── Views ────────────────────────────────────────────
        var views = await _context.HotelViews
            .Where(v => v.HotelId == hotelId)
            .ToListAsync();

        var totalViews = views.Sum(v => v.Views);
        var totalClicks = views.Sum(v => v.Clicks);
        var ctr = totalViews > 0
            ? Math.Round((double)totalClicks / totalViews * 100, 2)
            : 0;

        // ─── Monthly Revenue (آخر 12 شهر) ────────────────────
        var monthly = paidBookings
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new PeriodRevenueDto(
                $"{g.Key.Year}-{g.Key.Month:D2}",
                g.Sum(b => b.TotalAmount),
                g.Sum(b => b.OwnerAmount),
                g.Count()
            )).ToList();

        // ─── Quarterly Revenue ────────────────────────────────
        var quarterly = paidBookings
            .GroupBy(b => new { b.CreatedAt.Year, Quarter = (b.CreatedAt.Month - 1) / 3 + 1 })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Quarter)
            .Select(g => new PeriodRevenueDto(
                $"Q{g.Key.Quarter} {g.Key.Year}",
                g.Sum(b => b.TotalAmount),
                g.Sum(b => b.OwnerAmount),
                g.Count()
            )).ToList();

        // ─── Yearly Revenue ───────────────────────────────────
        var yearly = paidBookings
            .GroupBy(b => b.CreatedAt.Year)
            .OrderBy(g => g.Key)
            .Select(g => new PeriodRevenueDto(
                $"{g.Key}",
                g.Sum(b => b.TotalAmount),
                g.Sum(b => b.OwnerAmount),
                g.Count()
            )).ToList();

        return new OwnerDashboardDto(
            hotelId,
            hotel.Name,
            new RevenueDto(grossRevenue, platformFee, netRevenue, cancelLosses),
            new BookingStatsDto(
                bookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Confirmed),
                cancelledBookings.Count,
                bookings.Count(b => b.Status == BookingStatus.Completed),
                bookings.Count(b => b.Status == BookingStatus.Pending)
            ),
            new ViewStatsDto(totalViews, totalClicks, ctr),
            monthly,
            quarterly,
            yearly
        );
    }

    public async Task<DateRangeRevenueDto> GetRevenueByDateRangeAsync(
        long callerId, bool isAdmin, DateRangeRevenueRequest request)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == request.HotelId)
            ?? throw new HotelNotFoundException(request.HotelId);

        if (hotel.OwnerId != callerId && !isAdmin)
            throw new UnAuthoraizedOwnerException();

        var bookings = await _context.Bookings
            .Where(b => b.HotelId == request.HotelId &&
                        DateOnly.FromDateTime(b.CreatedAt) >= request.From &&
                        DateOnly.FromDateTime(b.CreatedAt) <= request.To)
            .ToListAsync();

        var paidBookings = bookings.Where(b =>
            b.Status == BookingStatus.Confirmed ||
            b.Status == BookingStatus.Completed).ToList();

        return new DateRangeRevenueDto(
            request.From,
            request.To,
            paidBookings.Sum(b => b.TotalAmount),
            paidBookings.Sum(b => b.PlatformFee),
            paidBookings.Sum(b => b.OwnerAmount),
            paidBookings.Count,
            bookings.Count(b => b.Status == BookingStatus.Cancelled)
        );
    }

    public async Task TrackViewAsync(long hotelId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var record = await _context.HotelViews
            .FirstOrDefaultAsync(v => v.HotelId == hotelId && v.Date == today);

        if (record is null)
        {
            _context.HotelViews.Add(new HotelView
            {
                HotelId = hotelId,
                Date = today,
                Views = 1,
                Clicks = 0
            });
        }
        else
        {
            record.Views++;
        }

        await _context.SaveChangesAsync();
    }

    public async Task TrackClickAsync(long hotelId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var record = await _context.HotelViews
            .FirstOrDefaultAsync(v => v.HotelId == hotelId && v.Date == today);

        if (record is null)
        {
            _context.HotelViews.Add(new HotelView
            {
                HotelId = hotelId,
                Date = today,
                Views = 0,
                Clicks = 1
            });
        }
        else
        {
            record.Clicks++;
        }

        await _context.SaveChangesAsync();
    }
}
