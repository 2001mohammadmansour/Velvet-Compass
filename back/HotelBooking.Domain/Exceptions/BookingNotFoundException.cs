namespace HotelBooking.Domain.Exceptions
{
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException(long bookingId)
            : base($"Booking with id {bookingId} was not found.") { }
    }
}
