using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-15): please review. Owner-defined date-range pricing rule, scoped to
    // the whole hotel (applies to every room type in it) — originally per-room-type, moved to
    // hotel scope so an owner sets a holiday window once instead of once per room type. Rules for
    // the same hotel may never have overlapping date ranges (enforced in HotelPricingService) so at
    // most one rule ever applies to a given night.
    public class SeasonalPriceRule : BaseEntity
    {
        public long HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public PriceAdjustmentType AdjustmentType { get; set; } = PriceAdjustmentType.Percentage;
        public decimal AdjustmentValue { get; set; }

        public Hotel Hotel { get; set; } = null!;
    }
}
