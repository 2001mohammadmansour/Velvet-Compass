namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown when an owner looks up, updates, or
    // deletes a seasonal price rule or occupancy price tier by id that doesn't exist (or doesn't
    // belong to their room type).
    public class PricingRuleNotFoundException : Exception
    {
        public PricingRuleNotFoundException(long ruleId)
            : base($"Pricing rule {ruleId} was not found.") { }
    }
}
