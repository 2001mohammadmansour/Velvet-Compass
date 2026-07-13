namespace HotelBooking.Application.DTOs.Users
{
    // CHANGED BY AI (2026-07-13): please review. New DTO for the admin user-list screen.
    public record AdminUserSummaryDto
    (
        long Id,
        string? Username,
        string? Email,
        string? PhoneNumber,
        string Role,
        DateTime CreatedAt,
        int BookingsCount,
        decimal AmountPaidToPlatform,
        List<string> OwnedHotelNames,
        // CHANGED BY AI (2026-07-13): please review. Added for the suspend/unsuspend feature.
        bool IsSuspended,
        DateTimeOffset? SuspendedUntil
    );
}
