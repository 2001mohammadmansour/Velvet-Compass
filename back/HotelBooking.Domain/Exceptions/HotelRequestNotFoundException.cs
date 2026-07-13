namespace HotelBooking.Domain.Exceptions
{
    public class HotelRequestNotFoundException : Exception
    {
        public HotelRequestNotFoundException(long requestId)
            : base($"Hotel request with id {requestId} was not found.") { }
    }
}
