namespace HotelBooking.Application.DTOs.Auth
{
    public record AuthResponse(
    long UserId,
    string Username,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
        );
}
