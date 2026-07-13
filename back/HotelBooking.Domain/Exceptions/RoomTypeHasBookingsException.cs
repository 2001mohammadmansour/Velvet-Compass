namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. New exception for blocking deletion of a room
    // type that has booking history — see RoomTypeServices.DeleteAsync.
    public class RoomTypeHasBookingsException : Exception
    {
        public RoomTypeHasBookingsException()
            : base("This room type has existing bookings and cannot be deleted.") { }
    }
}
