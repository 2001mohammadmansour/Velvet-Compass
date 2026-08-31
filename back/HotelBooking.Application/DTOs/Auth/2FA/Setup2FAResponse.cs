namespace HotelBooking.Application.DTOs.Auth._2FA
{
    public record Setup2FAResponse
    (
        string ManualEntryKey,
        string QrCodeImageBase64
        );
}
