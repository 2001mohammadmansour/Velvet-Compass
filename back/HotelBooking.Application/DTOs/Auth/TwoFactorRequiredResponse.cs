namespace HotelBooking.Application.DTOs.Auth
{
    // Returned by POST /api/v1/auth/login when the account has 2FA enabled: the client must
    // then call POST /api/v1/auth/2fa/verify with this challenge token.
    public record TwoFactorRequiredResponse(bool RequiresTwoFactor, string ChallengeToken);
}
