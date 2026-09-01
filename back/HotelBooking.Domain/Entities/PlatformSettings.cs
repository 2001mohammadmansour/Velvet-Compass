namespace HotelBooking.Domain.Entities
{
    // Single-row table holding platform-wide config. Currently just the platform's own Sham Cash
    // wallet, which owners send their commission payments to.
    public class PlatformSettings : BaseEntity
    {
        public string? ShamCashWallet { get; set; }
        public string? ShamCashQrUrl { get; set; }
    }
}
