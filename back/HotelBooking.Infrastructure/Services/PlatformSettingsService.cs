using HotelBooking.Application.DTOs.Platform;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class PlatformSettingsService : IPlatformSettingsService
    {
        private readonly AppDbContext _context;
        public PlatformSettingsService(AppDbContext context) => _context = context;

        private async Task<PlatformSettings> GetOrCreateAsync()
        {
            var row = await _context.PlatformSettings.FirstOrDefaultAsync();
            if (row is null)
            {
                row = new PlatformSettings();
                _context.PlatformSettings.Add(row);
                await _context.SaveChangesAsync();
            }
            return row;
        }

        public async Task<PlatformSettingsDto> GetAsync()
        {
            var row = await GetOrCreateAsync();
            return new PlatformSettingsDto(row.ShamCashWallet, row.ShamCashQrUrl);
        }

        public async Task<PlatformSettingsDto> UpdateShamCashWalletAsync(string? wallet)
        {
            var row = await GetOrCreateAsync();
            row.ShamCashWallet = string.IsNullOrWhiteSpace(wallet) ? null : wallet.Trim();
            await _context.SaveChangesAsync();
            return new PlatformSettingsDto(row.ShamCashWallet, row.ShamCashQrUrl);
        }

        public async Task<PlatformSettingsDto> SetShamCashQrAsync(string url)
        {
            var row = await GetOrCreateAsync();
            row.ShamCashQrUrl = url;
            await _context.SaveChangesAsync();
            return new PlatformSettingsDto(row.ShamCashWallet, row.ShamCashQrUrl);
        }
    }
}
