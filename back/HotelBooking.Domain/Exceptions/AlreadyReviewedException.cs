namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. Thrown when a guest tries to submit a second
    // review for a booking that already has one (one review per booking, enforced by a unique
    // index on Reviews.BookingId).
    public class AlreadyReviewedException : Exception
    {
        public AlreadyReviewedException(long bookingId)
            : base($"Booking {bookingId} has already been reviewed.") { }
    }
}
