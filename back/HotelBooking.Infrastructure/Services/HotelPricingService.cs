using HotelBooking.Application.DTOs.Pricing;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-15): please review. Owner-only management of a hotel's seasonal price
    // rules and occupancy price tiers — moved from per-room-type to hotel scope (an owner sets a
    // holiday window or demand tier once for the whole hotel instead of once per room type).
    // Deliberately has NO isAdmin parameter anywhere (unlike every other owner-editable resource in
    // this app) — per the explicit "admin not involved in pricing at all" requirement, ownership is
    // checked strictly against the caller.
    public class HotelPricingService : IHotelPricingService
    {
        private readonly AppDbContext _context;
        public HotelPricingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SeasonalPriceRuleDto>> GetSeasonalRulesAsync(long callerId, long hotelId)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            return await _context.SeasonalPriceRules
                .Where(r => r.HotelId == hotelId)
                .OrderBy(r => r.StartDate)
                .Select(r => MapSeasonalDto(r))
                .ToListAsync();
        }

        public async Task<SeasonalPriceRuleDto> CreateSeasonalRuleAsync(long callerId, long hotelId, CreateSeasonalPriceRuleRequest request)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var minBasePrice = await GetMinBasePriceAsync(hotelId);
            var adjustmentType = ValidateSeasonalRuleInput(minBasePrice, request.StartDate, request.EndDate, request.AdjustmentType, request.AdjustmentValue);
            await EnsureNoOverlapAsync(hotelId, request.StartDate, request.EndDate, excludingRuleId: null);

            var rule = new SeasonalPriceRule
            {
                HotelId = hotelId,
                Name = request.Name.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                AdjustmentType = adjustmentType,
                AdjustmentValue = request.AdjustmentValue
            };
            _context.SeasonalPriceRules.Add(rule);
            await _context.SaveChangesAsync();
            return MapSeasonalDto(rule);
        }

        public async Task<SeasonalPriceRuleDto> UpdateSeasonalRuleAsync(long callerId, long hotelId, long ruleId, UpdateSeasonalPriceRuleRequest request)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var rule = await _context.SeasonalPriceRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.HotelId == hotelId)
                ?? throw new PricingRuleNotFoundException(ruleId);

            var minBasePrice = await GetMinBasePriceAsync(hotelId);
            var adjustmentType = ValidateSeasonalRuleInput(minBasePrice, request.StartDate, request.EndDate, request.AdjustmentType, request.AdjustmentValue);
            await EnsureNoOverlapAsync(hotelId, request.StartDate, request.EndDate, excludingRuleId: ruleId);

            rule.Name = request.Name.Trim();
            rule.StartDate = request.StartDate;
            rule.EndDate = request.EndDate;
            rule.AdjustmentType = adjustmentType;
            rule.AdjustmentValue = request.AdjustmentValue;

            await _context.SaveChangesAsync();
            return MapSeasonalDto(rule);
        }

        public async Task DeleteSeasonalRuleAsync(long callerId, long hotelId, long ruleId)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var rule = await _context.SeasonalPriceRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.HotelId == hotelId)
                ?? throw new PricingRuleNotFoundException(ruleId);

            _context.SeasonalPriceRules.Remove(rule);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OccupancyPriceTierDto>> GetOccupancyTiersAsync(long callerId, long hotelId)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            return await _context.OccupancyPriceTiers
                .Where(t => t.HotelId == hotelId)
                .OrderBy(t => t.MinOccupancyPercent)
                .Select(t => MapOccupancyDto(t))
                .ToListAsync();
        }

        public async Task<OccupancyPriceTierDto> CreateOccupancyTierAsync(long callerId, long hotelId, CreateOccupancyPriceTierRequest request)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var minBasePrice = await GetMinBasePriceAsync(hotelId);
            var adjustmentType = ValidateOccupancyTierInput(minBasePrice, request.MinOccupancyPercent, request.AdjustmentType, request.AdjustmentValue);
            await EnsureUniqueThresholdAsync(hotelId, request.MinOccupancyPercent, excludingTierId: null);

            var tier = new OccupancyPriceTier
            {
                HotelId = hotelId,
                MinOccupancyPercent = request.MinOccupancyPercent,
                AdjustmentType = adjustmentType,
                AdjustmentValue = request.AdjustmentValue
            };
            _context.OccupancyPriceTiers.Add(tier);
            await _context.SaveChangesAsync();
            return MapOccupancyDto(tier);
        }

        public async Task<OccupancyPriceTierDto> UpdateOccupancyTierAsync(long callerId, long hotelId, long tierId, UpdateOccupancyPriceTierRequest request)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var tier = await _context.OccupancyPriceTiers.FirstOrDefaultAsync(t => t.Id == tierId && t.HotelId == hotelId)
                ?? throw new PricingRuleNotFoundException(tierId);

            var minBasePrice = await GetMinBasePriceAsync(hotelId);
            var adjustmentType = ValidateOccupancyTierInput(minBasePrice, request.MinOccupancyPercent, request.AdjustmentType, request.AdjustmentValue);
            await EnsureUniqueThresholdAsync(hotelId, request.MinOccupancyPercent, excludingTierId: tierId);

            tier.MinOccupancyPercent = request.MinOccupancyPercent;
            tier.AdjustmentType = adjustmentType;
            tier.AdjustmentValue = request.AdjustmentValue;

            await _context.SaveChangesAsync();
            return MapOccupancyDto(tier);
        }

        public async Task DeleteOccupancyTierAsync(long callerId, long hotelId, long tierId)
        {
            await GetOwnedHotelAsync(callerId, hotelId);
            var tier = await _context.OccupancyPriceTiers.FirstOrDefaultAsync(t => t.Id == tierId && t.HotelId == hotelId)
                ?? throw new PricingRuleNotFoundException(tierId);

            _context.OccupancyPriceTiers.Remove(tier);
            await _context.SaveChangesAsync();
        }

        // CHANGED BY AI (2026-07-15): please review. Deterministic default calendar anchored to
        // "today" (not hardcoded absolute dates) so this keeps producing sensible windows for a
        // hotel that signs up next year too. Generates a Summer Peak and a Winter Holidays window
        // for this year and next, skipping/clipping anything already fully or partially in the
        // past. Plus two fixed default occupancy tiers. These are ordinary rows once created — an
        // owner edits/deletes them exactly like a rule they made themselves.
        public async Task SeedDefaultsAsync(long hotelId, DateOnly today)
        {
            var rules = new List<SeasonalPriceRule>();
            foreach (var year in new[] { today.Year, today.Year + 1 })
            {
                var summerStart = new DateOnly(year, 7, 1);
                var summerEnd = new DateOnly(year, 8, 31);
                if (summerEnd >= today)
                {
                    var start = summerStart < today ? today : summerStart;
                    rules.Add(new SeasonalPriceRule
                    {
                        HotelId = hotelId,
                        Name = $"Summer Peak Season {year}",
                        StartDate = start,
                        EndDate = summerEnd,
                        AdjustmentType = PriceAdjustmentType.Percentage,
                        AdjustmentValue = 20m
                    });
                }

                var winterStart = new DateOnly(year, 12, 20);
                var winterEnd = new DateOnly(year + 1, 1, 5);
                if (winterEnd >= today)
                {
                    var start = winterStart < today ? today : winterStart;
                    rules.Add(new SeasonalPriceRule
                    {
                        HotelId = hotelId,
                        Name = $"Winter Holidays {year}",
                        StartDate = start,
                        EndDate = winterEnd,
                        AdjustmentType = PriceAdjustmentType.Percentage,
                        AdjustmentValue = 35m
                    });
                }
            }
            _context.SeasonalPriceRules.AddRange(rules.Where(r => r.StartDate <= r.EndDate));

            _context.OccupancyPriceTiers.AddRange(
                new OccupancyPriceTier { HotelId = hotelId, MinOccupancyPercent = 70, AdjustmentType = PriceAdjustmentType.Percentage, AdjustmentValue = 15m },
                new OccupancyPriceTier { HotelId = hotelId, MinOccupancyPercent = 90, AdjustmentType = PriceAdjustmentType.Percentage, AdjustmentValue = 35m }
            );

            await _context.SaveChangesAsync();
        }

        private async Task GetOwnedHotelAsync(long callerId, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);
            if (hotel.OwnerId != callerId)
                throw new UnAuthoraizedOwnerException();
        }

        // CHANGED BY AI (2026-07-15): please review. Since a rule now applies across every room
        // type in the hotel (each with its own BasePrice), a Flat adjustment is validated against
        // the CHEAPEST room type so it can't silently drive that room type's price to zero or
        // below. A hotel with no room types yet has nothing to protect, so the check is skipped.
        private async Task<decimal?> GetMinBasePriceAsync(long hotelId)
            => await _context.RoomTypes.Where(rt => rt.HotelId == hotelId).Select(rt => (decimal?)rt.BasePrice).MinAsync();

        private static PriceAdjustmentType ValidateSeasonalRuleInput(decimal? minBasePrice, DateOnly startDate, DateOnly endDate, string adjustmentType, decimal adjustmentValue)
        {
            if (startDate > endDate)
                throw new InvalidPricingRuleException("Start date must be on or before the end date.");
            if (startDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new InvalidPricingRuleException("Start date can't be in the past.");

            return ValidateAdjustment(minBasePrice, adjustmentType, adjustmentValue);
        }

        private static PriceAdjustmentType ValidateOccupancyTierInput(decimal? minBasePrice, int minOccupancyPercent, string adjustmentType, decimal adjustmentValue)
        {
            if (minOccupancyPercent is < 1 or > 100)
                throw new InvalidPricingRuleException("Occupancy threshold must be between 1 and 100.");

            return ValidateAdjustment(minBasePrice, adjustmentType, adjustmentValue);
        }

        private static PriceAdjustmentType ValidateAdjustment(decimal? minBasePrice, string adjustmentType, decimal adjustmentValue)
        {
            PriceAdjustmentType parsed;
            try
            {
                parsed = Enum.Parse<PriceAdjustmentType>(adjustmentType, ignoreCase: true);
            }
            catch (Exception)
            {
                throw new InvalidPricingRuleException($"Invalid adjustment type '{adjustmentType}'.");
            }

            if (parsed == PriceAdjustmentType.Percentage && adjustmentValue <= -100m)
                throw new InvalidPricingRuleException("A percentage decrease can't be 100% or more.");
            if (parsed == PriceAdjustmentType.Flat && minBasePrice.HasValue && minBasePrice.Value + adjustmentValue <= 0m)
                throw new InvalidPricingRuleException("This flat adjustment would make your cheapest room type's price zero or negative.");

            return parsed;
        }

        private async Task EnsureNoOverlapAsync(long hotelId, DateOnly startDate, DateOnly endDate, long? excludingRuleId)
        {
            var overlaps = await _context.SeasonalPriceRules
                .Where(r => r.HotelId == hotelId
                    && (excludingRuleId == null || r.Id != excludingRuleId)
                    && r.StartDate <= endDate && r.EndDate >= startDate)
                .AnyAsync();
            if (overlaps)
                throw new OverlappingPriceRuleException();
        }

        private async Task EnsureUniqueThresholdAsync(long hotelId, int minOccupancyPercent, long? excludingTierId)
        {
            var exists = await _context.OccupancyPriceTiers
                .Where(t => t.HotelId == hotelId
                    && (excludingTierId == null || t.Id != excludingTierId)
                    && t.MinOccupancyPercent == minOccupancyPercent)
                .AnyAsync();
            if (exists)
                throw new InvalidPricingRuleException($"A tier at {minOccupancyPercent}% already exists for this hotel.");
        }

        private static SeasonalPriceRuleDto MapSeasonalDto(SeasonalPriceRule r) => new(
            r.Id, r.HotelId, r.Name, r.StartDate, r.EndDate, r.AdjustmentType.ToString(), r.AdjustmentValue
        );

        private static OccupancyPriceTierDto MapOccupancyDto(OccupancyPriceTier t) => new(
            t.Id, t.HotelId, t.MinOccupancyPercent, t.AdjustmentType.ToString(), t.AdjustmentValue
        );
    }
}
