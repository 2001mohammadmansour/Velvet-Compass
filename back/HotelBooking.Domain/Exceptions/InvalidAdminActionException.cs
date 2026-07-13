namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. New exception for blocked admin user-management
    // actions: acting on your own account, acting on another Admin, or deleting a user who still
    // has bookings/hotels attached.
    public class InvalidAdminActionException : Exception
    {
        public InvalidAdminActionException(string message) : base(message) { }
    }
}
