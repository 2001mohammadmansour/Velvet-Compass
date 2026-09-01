using HotelBooking.Application.DTOs.Commission;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class CommissionService : ICommissionService
    {
        private const decimal Rate = 0.15m;

        private readonly AppDbContext _context;
        public CommissionService(AppDbContext context) => _context = context;

        // What the owner actually keeps from the guest — the base the 15% is charged on.
        // Only "finalised" bookings have a base: the stay happened, or it was cancelled.
        private static decimal KeptAmount(Booking b, DateOnly today)
        {
            if (b.Status == BookingStatus.Cancelled)
                return b.CancellationPenalty ?? 0m;
            if (b.Status == BookingStatus.Confirmed && b.CheckoutDate < today)
                return b.TotalAmount;
            return 0m; // not finalised yet
        }

        public async Task<CommissionSummaryDto> GetForHotelAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);
            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            var bookings = await _context.Bookings.Where(b => b.HotelId == hotelId).ToListAsync();
            return Summarise(hotel, bookings);
        }

        public async Task<CommissionSummaryDto> ClaimAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);
            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var bookings = await _context.Bookings.Where(b => b.HotelId == hotelId).ToListAsync();

            var toClaim = bookings.Where(b =>
                b.CommissionClaimedAt == null && b.CommissionPaidAt == null && KeptAmount(b, today) > 0m).ToList();

            if (toClaim.Count == 0)
                throw new InvalidRequestException("There is no commission to pay right now.");

            var now = DateTime.UtcNow;
            foreach (var b in toClaim)
            {
                b.CommissionAmount = Math.Round(KeptAmount(b, today) * Rate, 2);
                b.CommissionClaimedAt = now;
            }
            await _context.SaveChangesAsync();

            return Summarise(hotel, bookings);
        }

        public async Task<CommissionSummaryDto> ConfirmAsync(long adminId, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            var bookings = await _context.Bookings.Where(b => b.HotelId == hotelId).ToListAsync();
            var toConfirm = bookings.Where(b => b.CommissionClaimedAt != null && b.CommissionPaidAt == null).ToList();

            if (toConfirm.Count == 0)
                throw new InvalidRequestException("This hotel has no commission payment awaiting confirmation.");

            var now = DateTime.UtcNow;
            foreach (var b in toConfirm)
                b.CommissionPaidAt = now;
            await _context.SaveChangesAsync();

            return Summarise(hotel, bookings);
        }

        public async Task<PlatformCommissionDto> GetPlatformOverviewAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var hotels = await _context.Hotels.Include(h => h.Owner).ToListAsync();
            var bookings = await _context.Bookings.ToListAsync();
            var byHotel = bookings.GroupBy(b => b.HotelId).ToDictionary(g => g.Key, g => g.ToList());

            var rows = new List<HotelCommissionRowDto>();
            decimal pending = 0m, collected = 0m;

            foreach (var hotel in hotels)
            {
                var hb = byHotel.TryGetValue(hotel.Id, out var list) ? list : new List<Booking>();
                var owed = Math.Round(hb
                    .Where(b => b.CommissionClaimedAt == null && b.CommissionPaidAt == null)
                    .Sum(b => KeptAmount(b, today)) * Rate, 2);
                var awaiting = hb.Where(b => b.CommissionClaimedAt != null && b.CommissionPaidAt == null)
                    .Sum(b => b.CommissionAmount ?? 0m);
                var paid = hb.Where(b => b.CommissionPaidAt != null).Sum(b => b.CommissionAmount ?? 0m);

                pending += owed + awaiting;
                collected += paid;

                if (owed > 0m || awaiting > 0m)
                    rows.Add(new HotelCommissionRowDto(hotel.Id, hotel.Name, hotel.Owner?.UserName ?? "", owed, awaiting));
            }

            return new PlatformCommissionDto(pending, collected,
                rows.OrderByDescending(r => r.Owed + r.AwaitingConfirmation).ToList());
        }

        private static CommissionSummaryDto Summarise(Hotel hotel, List<Booking> bookings)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var owedList = bookings.Where(b =>
                b.CommissionClaimedAt == null && b.CommissionPaidAt == null && KeptAmount(b, today) > 0m).ToList();
            var awaitingList = bookings.Where(b => b.CommissionClaimedAt != null && b.CommissionPaidAt == null).ToList();

            var owed = Math.Round(owedList.Sum(b => KeptAmount(b, today)) * Rate, 2);
            var awaiting = awaitingList.Sum(b => b.CommissionAmount ?? 0m);
            var paid = bookings.Where(b => b.CommissionPaidAt != null).Sum(b => b.CommissionAmount ?? 0m);

            return new CommissionSummaryDto(
                hotel.Id, hotel.Name, owed, awaiting, paid, owedList.Count, awaitingList.Count);
        }
    }
}
