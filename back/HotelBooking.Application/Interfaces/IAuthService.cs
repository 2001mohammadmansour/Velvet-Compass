using HotelBooking.Application.DTOs.Auth;

namespace HotelBooking.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
        // CHANGED BY AI (2026-07-13): please review. New self-service profile methods backing the
        // Edit Profile feature.
        Task<UserProfileDto> GetMyProfileAsync(long userId);
        Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
    }
}
