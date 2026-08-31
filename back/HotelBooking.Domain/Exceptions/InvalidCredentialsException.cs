namespace HotelBooking.Domain.Exceptions
{
    // Thrown when a login / password check fails. Mapped to HTTP 401 by ExceptionMiddleware.
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }
}
