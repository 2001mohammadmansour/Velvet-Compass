namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. New exception for the admin user-management
    // actions (suspend/unsuspend/delete).
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(long userId)
            : base($"User with id {userId} was not found.") { }
    }
}
