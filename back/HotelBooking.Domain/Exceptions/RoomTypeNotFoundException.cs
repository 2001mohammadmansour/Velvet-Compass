namespace HotelBooking.Domain.Exceptions
{
    public class RoomTypeNotFoundException : Exception
    {
        public RoomTypeNotFoundException(long roomTypeId)
        : base($"Room type with id {roomTypeId} was not found") { }
    }
}
