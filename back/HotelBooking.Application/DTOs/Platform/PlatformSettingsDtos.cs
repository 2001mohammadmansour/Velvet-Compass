namespace HotelBooking.Application.DTOs.Platform
{
    public record PlatformSettingsDto(string? ShamCashWallet, string? ShamCashQrUrl);

    public record UpdatePlatformShamCashRequest(string? ShamCashWallet);
}
