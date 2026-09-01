using HotelBooking.Application.DTOs.Platform;

namespace HotelBooking.Application.Interfaces
{
    public interface IPlatformSettingsService
    {
        Task<PlatformSettingsDto> GetAsync();
        Task<PlatformSettingsDto> UpdateShamCashWalletAsync(string? wallet);
        Task<PlatformSettingsDto> SetShamCashQrAsync(string url);
    }
}
