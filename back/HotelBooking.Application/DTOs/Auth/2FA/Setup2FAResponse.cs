namespace HotelBooking.Application.DTOs.Auth
{
    public record Setup2FAResponse
    (
        string ManualEntryKey,
        string QrCodeImageBase64
        );
}
