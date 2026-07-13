namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. Thrown when an admin looks up or deletes a
    // review by id that doesn't exist.
    public class ReviewNotFoundException : Exception
    {
        public ReviewNotFoundException(long reviewId)
            : base($"Review {reviewId} was not found.") { }
    }
}
