namespace HotelBooking.Domain.Exceptions
{
    public class RoomNotFoundException : Exception
    {
        public RoomNotFoundException(long roomId)
            : base($"Room with id {roomId} was not found.") { }
    }
}
