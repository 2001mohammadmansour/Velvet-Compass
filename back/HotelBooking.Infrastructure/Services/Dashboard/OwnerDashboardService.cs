using HotelBooking.Application.DTOs.Dashboard;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Common;
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

    private sealed class RoomAgg
    {
        public int Booked;
        public int RoomNights;
        public int Cancelled;
        public decimal Revenue;
        public int StayNights;
        public int Stays;
    }

    public async Task<List<RoomPerformanceDto>> GetRoomPerformanceAsync(
        long callerId, bool isAdmin, long hotelId, DateOnly? from, DateOnly? to)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
            ?? throw new HotelNotFoundException(hotelId);

        if (hotel.OwnerId != callerId && !isAdmin)
            throw new UnAuthoraizedOwnerException();

        var bookings = await _context.Bookings
            .Include(b => b.Items).ThenInclude(i => i.RoomType)
            .Where(b => b.HotelId == hotelId)
            .Where(b => (from == null || b.CheckinDate >= from) && (to == null || b.CheckinDate <= to))
            .ToListAsync();

        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .ToListAsync();

        var map = new Dictionary<string, RoomAgg>();
        RoomAgg Agg(string name)
        {
            if (!map.TryGetValue(name, out var a)) { a = new RoomAgg(); map[name] = a; }
            return a;
        }

        // Seed every room type so ones with no activity still show as zero rows.
        foreach (var rt in roomTypes) Agg(rt.Name);

        foreach (var b in bookings)
        {
            var paid = b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed;
            var cancelled = b.Status == BookingStatus.Cancelled;
            if (!paid && !cancelled) continue;

            foreach (var it in b.Items)
            {
                var a = Agg(it.RoomType?.Name ?? "—");
                if (cancelled)
                {
                    a.Cancelled += 1;
                }
                else
                {
                    a.Booked += 1;
                    a.RoomNights += it.Qty * it.Nights;
                    a.Revenue += it.TotalPrice;
                    a.StayNights += it.Nights;
                    a.Stays += 1;
                }
            }
        }

        var totalRevenue = map.Values.Sum(a => a.Revenue);

        return map
            .Select(kv =>
            {
                var a = kv.Value;
                var denom = a.Booked + a.Cancelled;
                return new RoomPerformanceDto(
                    kv.Key,
                    a.Booked,
                    a.RoomNights,
                    a.Cancelled,
                    denom > 0 ? Math.Round((double)a.Cancelled / denom, 4) : 0,
                    Math.Round(a.Revenue, 2),
                    totalRevenue > 0 ? Math.Round((double)(a.Revenue / totalRevenue), 4) : 0,
                    a.RoomNights > 0 ? Math.Round(a.Revenue / a.RoomNights, 2) : 0,
                    a.Stays > 0 ? Math.Round((double)a.StayNights / a.Stays, 1) : 0);
            })
            .OrderByDescending(r => r.RoomNights)
            .ToList();
    }

    public async Task<List<CalendarDayDto>> GetCalendarAsync(
        long callerId, bool isAdmin, long hotelId, DateOnly from, DateOnly to)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
            ?? throw new HotelNotFoundException(hotelId);

        if (hotel.OwnerId != callerId && !isAdmin)
            throw new UnAuthoraizedOwnerException();

        if (to < from) (from, to) = (to, from);
        if (to.DayNumber - from.DayNumber > 400) to = from.AddDays(400);

        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .Select(rt => new { rt.Id, rt.Name })
            .OrderBy(rt => rt.Name)
            .ToListAsync();

        var rooms = await _context.Rooms
            .Where(r => r.RoomType.HotelId == hotelId)
            .Select(r => new { r.Id, r.RoomTypeId, r.RoomNumber })
            .ToListAsync();

        var totalByType = rooms.GroupBy(r => r.RoomTypeId).ToDictionary(g => g.Key, g => g.Count());
        var roomTypeOfRoom = rooms.ToDictionary(r => r.Id, r => r.RoomTypeId);
        var roomNumberOfRoom = rooms.ToDictionary(r => r.Id, r => r.RoomNumber);
        var typeName = roomTypes.ToDictionary(rt => rt.Id, rt => rt.Name);

        var bookings = await _context.Bookings
            .Where(b => b.HotelId == hotelId
                        && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                        && b.CheckinDate <= to && b.CheckoutDate > from)
            .Select(b => new
            {
                b.CheckinDate,
                b.CheckoutDate,
                Items = b.Items.Select(i => new { i.RoomTypeId, i.Qty }).ToList()
            })
            .ToListAsync();

        var roomIds = rooms.Select(r => r.Id).ToHashSet();
        var blocked = (await _context.RoomAvailabilities
            .Where(a => a.Status == RoomAvailabilityStatus.Blocked && a.Date >= from && a.Date <= to)
            .Select(a => new { a.RoomId, a.Date })
            .ToListAsync())
            .Where(a => roomIds.Contains(a.RoomId))
            .ToList();

        var days = new List<CalendarDayDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var occByType = new Dictionary<long, int>();
            foreach (var b in bookings)
            {
                if (b.CheckinDate > d || d >= b.CheckoutDate) continue;
                foreach (var it in b.Items)
                    occByType[it.RoomTypeId] = occByType.GetValueOrDefault(it.RoomTypeId) + it.Qty;
            }

            var blkByType = new Dictionary<long, int>();
            var blockedRoomsToday = new List<CalendarBlockedRoomDto>();
            foreach (var bl in blocked)
            {
                if (bl.Date != d) continue;
                if (!roomTypeOfRoom.TryGetValue(bl.RoomId, out var rtId)) continue;
                blkByType[rtId] = blkByType.GetValueOrDefault(rtId) + 1;
                blockedRoomsToday.Add(new CalendarBlockedRoomDto(
                    roomNumberOfRoom.GetValueOrDefault(bl.RoomId, "—"),
                    typeName.GetValueOrDefault(rtId, "—")));
            }

            var typeRows = new List<CalendarRoomTypeDayDto>();
            foreach (var rt in roomTypes)
            {
                var total = totalByType.GetValueOrDefault(rt.Id);
                var occ = Math.Min(occByType.GetValueOrDefault(rt.Id), total);
                var blk = Math.Min(blkByType.GetValueOrDefault(rt.Id), Math.Max(0, total - occ));
                typeRows.Add(new CalendarRoomTypeDayDto(
                    rt.Id, rt.Name, total, occ, blk, Math.Max(0, total - occ - blk)));
            }

            days.Add(new CalendarDayDto(
                d,
                typeRows.Sum(r => r.TotalUnits),
                typeRows.Sum(r => r.OccupiedUnits),
                typeRows.Sum(r => r.BlockedUnits),
                typeRows.Sum(r => r.AvailableUnits),
                typeRows,
                blockedRoomsToday.OrderBy(r => r.RoomNumber).ToList()));
        }

        return days;
    }

    public async Task TrackViewAsync(long hotelId)
    {
        var today = SyriaClock.Today;

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
        var today = SyriaClock.Today;

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
