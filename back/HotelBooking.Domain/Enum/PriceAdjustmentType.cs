namespace HotelBooking.Domain.Enum
{
    // CHANGED BY AI (2026-07-15): please review. Shared by SeasonalPriceRule and
    // OccupancyPriceTier — whether an adjustment is a percentage of BasePrice or a flat dollar
    // amount per night. Named "Flat" (not "Fixed") to match CancellationFeeType's naming rather
    // than ExtraBedPriceType's inconsistent "Fixed" for the same concept.
    public enum PriceAdjustmentType
    {
        Percentage,
        Flat
    }
}
