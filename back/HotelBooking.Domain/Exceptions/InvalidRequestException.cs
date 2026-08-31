namespace HotelBooking.Domain.Exceptions
{
    // Thrown for invalid client input that isn't covered by a more specific exception
    // (duplicate email/username, empty username, rejected Identity result, bad 2FA code, …).
    // Mapped to HTTP 400 by ExceptionMiddleware.
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(string message) : base(message) { }
    }
}
