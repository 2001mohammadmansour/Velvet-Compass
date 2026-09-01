using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // One settlement run for one hotel for one period (e.g. "2026-01"). Bundles every booking
    // whose money matured in that period and records the single netted transfer between the
    // owner and the platform. Settled bookings point back here via Booking.SettlementId.
    public class Settlement : BaseEntity
    {
        public long HotelId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;   // "2026-01"

        public SettlementDirection Direction { get; set; }

        // Owed to the owner: their 85% share of the hotel's matured ONLINE bookings.
        public decimal OwnerCredit { get; set; }

        // Owed to the platform: its 15% commission on the hotel's matured CASH bookings.
        public decimal PlatformCommission { get; set; }

        // |OwnerCredit - PlatformCommission| — the one transfer that actually happens.
        public decimal NetAmount { get; set; }

        public int BookingCount { get; set; }
        public long CreatedByUserId { get; set; }

        public Hotel Hotel { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
