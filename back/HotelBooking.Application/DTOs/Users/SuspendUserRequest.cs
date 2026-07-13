namespace HotelBooking.Application.DTOs.Users
{
    // CHANGED BY AI (2026-07-13): please review. Until = null means suspended indefinitely.
    public record SuspendUserRequest(DateTimeOffset? Until);
}
