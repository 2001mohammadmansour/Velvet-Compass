using HotelBooking.Application.DTOs.Pricing;

namespace HotelBooking.Application.Interfaces
{
    // CHANGED BY AI (2026-07-15): please review. Owner-only CRUD for a hotel's seasonal price
    // rules and occupancy price tiers — scoped to the whole hotel (applies to every room type in
    // it), not per room type, so an owner configures a holiday window or demand tier once instead
    // of once per room type. Deliberately no isAdmin parameter anywhere here, per the explicit
    // "admin not involved in pricing at all" requirement (every other owner-editable resource in
    // this app allows an Admin bypass; this one intentionally does not).
    public interface IHotelPricingService
    {
        Task<List<SeasonalPriceRuleDto>> GetSeasonalRulesAsync(long callerId, long hotelId);
        Task<SeasonalPriceRuleDto> CreateSeasonalRuleAsync(long callerId, long hotelId, CreateSeasonalPriceRuleRequest request);
        Task<SeasonalPriceRuleDto> UpdateSeasonalRuleAsync(long callerId, long hotelId, long ruleId, UpdateSeasonalPriceRuleRequest request);
        Task DeleteSeasonalRuleAsync(long callerId, long hotelId, long ruleId);

        Task<List<OccupancyPriceTierDto>> GetOccupancyTiersAsync(long callerId, long hotelId);
        Task<OccupancyPriceTierDto> CreateOccupancyTierAsync(long callerId, long hotelId, CreateOccupancyPriceTierRequest request);
        Task<OccupancyPriceTierDto> UpdateOccupancyTierAsync(long callerId, long hotelId, long tierId, UpdateOccupancyPriceTierRequest request);
        Task DeleteOccupancyTierAsync(long callerId, long hotelId, long tierId);

        // CHANGED BY AI (2026-07-15): please review. Called once right after a new hotel is created
        // (HotelServices.CreateHotelAsync) to pre-populate sensible defaults — a default seasonal
        // calendar (summer/winter windows for this year and next) and two default occupancy tiers
        // (70%/90%). Not called with any auth context since it runs internally as part of hotel
        // creation, which is already authorized.
        Task SeedDefaultsAsync(long hotelId, DateOnly today);
    }
}
