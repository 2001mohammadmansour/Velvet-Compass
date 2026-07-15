namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown when a new/updated seasonal price rule's
    // date range would overlap another active rule on the same room type.
    public class OverlappingPriceRuleException : Exception
    {
        public OverlappingPriceRuleException()
            : base("This date range overlaps another seasonal pricing rule for this room type.") { }
    }
}
