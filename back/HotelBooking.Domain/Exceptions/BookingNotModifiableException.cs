namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. Thrown when a guest tries to modify a booking's
    // dates but it's cancelled/completed, or its check-in date has already passed.
    public class BookingNotModifiableException : Exception
    {
        public BookingNotModifiableException(string message) : base(message) { }
    }
}
