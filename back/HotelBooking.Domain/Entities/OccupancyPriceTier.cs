using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-15): please review. Owner-defined demand-based surge tier, scoped to
    // the whole hotel (originally per-room-type, moved to hotel scope so an owner configures one
    // set of tiers instead of once per room type). Applied per room type at resolution time using
    // that room type's own occupancy (or the hotel's overall occupancy as a fallback for room types
    // with a small physical pool — see RoomPricingService). MinOccupancyPercent must be unique per
    // hotel (enforced in HotelPricingService); the resolver picks the highest threshold the actual
    // occupancy still meets, so at most one tier applies to a given night.
    public class OccupancyPriceTier : BaseEntity
    {
        public long HotelId { get; set; }
        public int MinOccupancyPercent { get; set; }
        public PriceAdjustmentType AdjustmentType { get; set; } = PriceAdjustmentType.Percentage;
        public decimal AdjustmentValue { get; set; }

        public Hotel Hotel { get; set; } = null!;
    }
}
