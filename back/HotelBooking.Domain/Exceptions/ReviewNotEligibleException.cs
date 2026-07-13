namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. Thrown when a booking isn't eligible for a
    // review yet (cancelled, or the checkout date hasn't passed yet).
    public class ReviewNotEligibleException : Exception
    {
        public ReviewNotEligibleException(string message) : base(message) { }
    }
}
