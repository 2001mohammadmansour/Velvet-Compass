namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown for invalid seasonal price rule / occupancy
    // price tier input (bad date range, out-of-range percentage, duplicate occupancy threshold,
    // etc).
    public class InvalidPricingRuleException : Exception
    {
        public InvalidPricingRuleException(string message) : base(message) { }
    }
}
