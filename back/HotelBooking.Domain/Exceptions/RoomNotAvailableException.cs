namespace HotelBooking.Domain.Exceptions
{
    public class RoomNotAvailableException : Exception
    {
        public RoomNotAvailableException()
            : base($"No rooms available for the selected dates.") { }
    }
}
