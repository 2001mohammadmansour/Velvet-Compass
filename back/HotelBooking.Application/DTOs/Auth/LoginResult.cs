namespace HotelBooking.Application.DTOs.Auth;

public record LoginResult(
    bool RequiresTwoFactor,
    string? ChallengeToken,
    AuthResponse? Auth
);
