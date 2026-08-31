using HotelBooking.Application.DTOs.Auth;

namespace HotelBooking.Application.Interfaces
{
    // Credential / token / session concerns only. Everything about the user record itself
    // (profile, change password, admin management) lives in IUserService.
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
    }
}
