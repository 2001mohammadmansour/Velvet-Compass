using HotelBooking.Application.DTOs.Pricing;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-15): please review. Resolves the actual nightly price for a stay,
    // combining (in precedence order, highest first): a manual per-room PriceOverride for that
    // specific night (the pre-existing, still-unexposed RoomAvailability mechanism) > the hotel's
    // seasonal price rule for that night, if any > the hotel's occupancy price tier for that night,
    // if any (seasonal and occupancy STACK when both apply — seasonal first, then occupancy surge
    // on top) > plain BasePrice. Seasonal rules and occupancy tiers are hotel-scoped (one calendar/
    // tier set per hotel, not per room type), but the occupancy % that triggers a tier is still
    // measured against the specific room type's own physical rooms when it has 5 or more; for
    // smaller pools it's measured hotel-wide instead (see GetNightlyPricesAsync). Used by both
    // BookingService (authoritative charge) and the public price-quote endpoint, so the two can
    // never drift apart.
    public class RoomPricingService : IRoomPricingService
    {
        private readonly AppDbContext _context;
        public RoomPricingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NightlyPriceDto>> GetNightlyPricesAsync(long roomId, long hotelId, long roomTypeId, decimal basePrice, DateOnly checkinDate, DateOnly checkoutDate)
        {
            var lastNight = checkoutDate.AddDays(-1);

            // Manual per-room overrides for this specific room, within the stay.
            var overridesByDate = await _context.RoomAvailabilities
                .Where(a => a.RoomId == roomId && a.Date >= checkinDate && a.Date <= lastNight && a.PriceOverride.HasValue)
                .ToDictionaryAsync(a => a.Date, a => a.PriceOverride!.Value);

            // Seasonal rules for this hotel that overlap the stay at all.
            var seasonalRules = await _context.SeasonalPriceRules
                .Where(r => r.HotelId == hotelId && r.StartDate <= lastNight && r.EndDate >= checkinDate)
                .ToListAsync();

            // Occupancy tiers for this hotel, highest threshold first so the first match wins.
            var occupancyTiers = await _context.OccupancyPriceTiers
                .Where(t => t.HotelId == hotelId)
                .OrderByDescending(t => t.MinOccupancyPercent)
                .ToListAsync();

            // Occupancy needs every physical room of this type, not just the one being priced.
            var roomIdsOfType = await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId)
                .Select(r => r.Id)
                .ToListAsync();
            var totalRoomsOfType = roomIdsOfType.Count;

            // CHANGED BY AI (2026-07-15): please review. Room types with a tiny physical pool (< 5
            // rooms) swing too wildly on their own occupancy % (e.g. 1/2 booked = 50%, already past
            // most tiers) to drive a meaningful surge. For those, fall back to the hotel's overall
            // occupancy across every room type as a steadier signal; room types with 5+ rooms keep
            // using their own occupancy, unchanged.
            const int SmallPoolThreshold = 5;
            var occupancyRoomIds = roomIdsOfType;
            var totalRoomsForOccupancy = totalRoomsOfType;

            if (occupancyTiers.Count > 0 && totalRoomsOfType > 0 && totalRoomsOfType < SmallPoolThreshold)
            {
                occupancyRoomIds = await _context.Rooms
                    .Where(r => r.RoomType.HotelId == hotelId)
                    .Select(r => r.Id)
                    .ToListAsync();
                totalRoomsForOccupancy = occupancyRoomIds.Count;
            }

            var occupiedByDate = occupancyTiers.Count == 0 || totalRoomsForOccupancy == 0
                ? new Dictionary<DateOnly, int>()
                : await _context.RoomAvailabilities
                    .Where(a => occupancyRoomIds.Contains(a.RoomId) && a.Date >= checkinDate && a.Date <= lastNight
                        && (a.Status == RoomAvailabilityStatus.Booked || a.Status == RoomAvailabilityStatus.Blocked))
                    .GroupBy(a => a.Date)
                    .Select(g => new { Date = g.Key, Count = g.Select(a => a.RoomId).Distinct().Count() })
                    .ToDictionaryAsync(x => x.Date, x => x.Count);

            var results = new List<NightlyPriceDto>();
            for (var date = checkinDate; date <= lastNight; date = date.AddDays(1))
            {
                if (overridesByDate.TryGetValue(date, out var overridePrice))
                {
                    results.Add(new NightlyPriceDto(date, Math.Round(Math.Max(0.01m, overridePrice), 2), null));
                    continue;
                }

                var price = basePrice;
                var reasons = new List<string>();

                var seasonalRule = seasonalRules.FirstOrDefault(r => r.StartDate <= date && r.EndDate >= date);
                if (seasonalRule != null)
                {
                    price = Apply(price, seasonalRule.AdjustmentType, seasonalRule.AdjustmentValue);
                    reasons.Add(seasonalRule.Name);
                }

                if (occupancyTiers.Count > 0 && totalRoomsForOccupancy > 0)
                {
                    occupiedByDate.TryGetValue(date, out var occupiedCount);
                    var occupancyPercent = (occupiedCount * 100) / totalRoomsForOccupancy;
                    var tier = occupancyTiers.FirstOrDefault(t => occupancyPercent >= t.MinOccupancyPercent);
                    if (tier != null)
                    {
                        price = Apply(price, tier.AdjustmentType, tier.AdjustmentValue);
                        reasons.Add("High Demand");
                    }
                }

                price = Math.Round(Math.Max(0.01m, price), 2);
                results.Add(new NightlyPriceDto(date, price, reasons.Count > 0 ? string.Join(" + ", reasons) : null));
            }

            return results;
        }

        public async Task<PriceQuoteDto> GetQuoteAsync(long hotelId, long roomTypeId, DateOnly checkinDate, DateOnly checkoutDate)
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            // No specific physical room is chosen yet at browse time — sentinel id 0 can never
            // match a real RoomAvailability row, so the per-room override tier is naturally skipped.
            var nights = await GetNightlyPricesAsync(0, hotelId, roomTypeId, roomType.BasePrice, checkinDate, checkoutDate);
            return new PriceQuoteDto(nights, nights.Sum(n => n.Price));
        }

        private static decimal Apply(decimal price, PriceAdjustmentType type, decimal value)
            => type == PriceAdjustmentType.Percentage ? price * (1 + value / 100m) : price + value;
    }
}
