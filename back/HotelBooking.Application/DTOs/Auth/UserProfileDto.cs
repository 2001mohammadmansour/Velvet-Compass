namespace HotelBooking.Application.DTOs.Auth
{
    // CHANGED BY AI (2026-07-13): please review. New DTOs for the self-service Edit Profile
    // feature. Email is deliberately never editable here (only returned for display).
    public record UserProfileDto(
        long Id,
        string Username,
        string Email,
        string? PhoneNumber,
        string Role,
        DateTime CreatedAt
    );

    public record UpdateProfileRequest(
        string Username,
        string? PhoneNumber
    );

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}
