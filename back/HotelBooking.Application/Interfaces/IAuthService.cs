using HotelBooking.Application.DTOs.Auth;
using HotelBooking.Application.DTOs.Auth._2FA;

namespace HotelBooking.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);

        // 2FA
        Task<Setup2FAResponse> SetupTwoFactorAsync(long userId);
        Task<Enable2FAResponse> EnableTwoFactorAsync(long userId, string code);
        Task DisableTwoFactorAsync(long userId, string password);
        Task<AuthResponse> VerifyTwoFactorAsync(Verify2FARequest request);
        Task<List<string>> RegenerateRecoveryCodesAsync(long userId);

        // CHANGED BY AI (2026-07-13): please review. New self-service profile methods backing the
        // Edit Profile feature.
        Task<UserProfileDto> GetMyProfileAsync(long userId);
        Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
    }
}
