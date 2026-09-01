using HotelBooking.Application.DTOs.Settlements;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly AppDbContext _context;
        public SettlementService(AppDbContext context) => _context = context;

        // "Matured, unsettled" — the stay has finished and the money hasn't been settled yet.
        private IQueryable<Booking> DueBookings()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return _context.Bookings.Where(b =>
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed) &&
                b.SettledAt == null &&
                b.CheckoutDate < today);
        }

        public async Task<List<SettlementPreviewDto>> GetPreviewAsync()
        {
            var due = await DueBookings()
                .Include(b => b.Hotel).ThenInclude(h => h.Owner)
                .ToListAsync();

            return due
                .GroupBy(b => b.Hotel)
                .Select(g => BuildPreview(g.Key, g.ToList()))
                .OrderByDescending(x => x.NetAmount)
                .ToList();
        }

        public async Task<SettlementDto> RunAsync(long adminUserId, RunSettlementRequest request)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == request.HotelId)
                ?? throw new HotelNotFoundException(request.HotelId);

            var due = await DueBookings().Where(b => b.HotelId == request.HotelId).ToListAsync();
            if (due.Count == 0)
                throw new InvalidRequestException("There is nothing to settle for this hotel yet.");

            var ownerCredit = due.Where(b => b.PaymentMethod == PaymentMethod.Online).Sum(b => b.OwnerAmount);
            var platformCommission = due.Where(b => b.PaymentMethod == PaymentMethod.CashOnArrival).Sum(b => b.PlatformFee);
            var direction = ownerCredit >= platformCommission
                ? SettlementDirection.PlatformToOwner
                : SettlementDirection.OwnerToPlatform;

            var settlement = new Settlement
            {
                HotelId = hotel.Id,
                PeriodLabel = string.IsNullOrWhiteSpace(request.PeriodLabel)
                    ? DateTime.UtcNow.ToString("yyyy-MM")
                    : request.PeriodLabel!.Trim(),
                Direction = direction,
                OwnerCredit = ownerCredit,
                PlatformCommission = platformCommission,
                NetAmount = Math.Abs(ownerCredit - platformCommission),
                BookingCount = due.Count,
                CreatedByUserId = adminUserId
            };

            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            foreach (var b in due)
            {
                b.SettledAt = now;
                b.SettlementId = settlement.Id;
            }
            await _context.SaveChangesAsync();

            return Map(settlement, hotel.Name);
        }

        public async Task<List<SettlementDto>> GetHistoryAsync(long? hotelId)
        {
            var query = _context.Settlements.Include(s => s.Hotel).AsQueryable();
            if (hotelId.HasValue)
                query = query.Where(s => s.HotelId == hotelId.Value);

            var rows = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return rows.Select(s => Map(s, s.Hotel.Name)).ToList();
        }

        public async Task<List<SettlementDto>> GetHotelHistoryAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            var rows = await _context.Settlements
                .Where(s => s.HotelId == hotelId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return rows.Select(s => Map(s, hotel.Name)).ToList();
        }

        private static SettlementPreviewDto BuildPreview(Hotel hotel, List<Booking> due)
        {
            var ownerCredit = due.Where(b => b.PaymentMethod == PaymentMethod.Online).Sum(b => b.OwnerAmount);
            var platformCommission = due.Where(b => b.PaymentMethod == PaymentMethod.CashOnArrival).Sum(b => b.PlatformFee);
            var direction = ownerCredit >= platformCommission
                ? SettlementDirection.PlatformToOwner
                : SettlementDirection.OwnerToPlatform;

            return new SettlementPreviewDto(
                hotel.Id,
                hotel.Name,
                hotel.Owner?.UserName ?? "",
                due.Count,
                ownerCredit,
                platformCommission,
                Math.Abs(ownerCredit - platformCommission),
                direction.ToString());
        }

        private static SettlementDto Map(Settlement s, string hotelName) => new(
            s.Id, s.HotelId, hotelName, s.PeriodLabel, s.Direction.ToString(),
            s.OwnerCredit, s.PlatformCommission, s.NetAmount, s.BookingCount, s.CreatedAt);
    }
}
