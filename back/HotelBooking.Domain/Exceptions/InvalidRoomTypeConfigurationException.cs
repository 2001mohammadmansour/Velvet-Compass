namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown when a room type's create/update request
    // has an invalid extra-bed configuration (e.g. MaxExtraBeds outside 0-2, negative prices).
    public class InvalidRoomTypeConfigurationException : Exception
    {
        public InvalidRoomTypeConfigurationException(string message) : base(message) { }
    }
}
