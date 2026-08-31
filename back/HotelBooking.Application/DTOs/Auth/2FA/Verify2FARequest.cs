namespace HotelBooking.Application.DTOs.Auth;

public record Verify2FARequest(string ChallengeToken, string? Code, string? RecoveryCode);
