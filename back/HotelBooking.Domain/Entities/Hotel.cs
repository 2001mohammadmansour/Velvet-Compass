using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    public class Hotel : BaseEntity
    {
        public long OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public short StarRating { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // CHANGED BY AI (2026-07-12): please review. New column for the owner-facing
        // "auto-accept bookings" toggle; see BookingService.CreateAsync for its effect.
        public bool AutoAcceptBookings { get; set; } = true;
        // CHANGED BY AI (2026-07-12): please review. New columns backing the owner-facing
        // breakfast add-on toggle/price; see BookingService.CreateAsync for how it's priced.
        public bool BreakfastAvailable { get; set; } = false;
        public decimal BreakfastPrice { get; set; } = 0m;

        // CHANGED BY AI (2026-07-13): please review. New columns backing a real, per-hotel
        // cancellation policy (previously the owner dashboard had a "free cancellation" toggle
        // that posted to a mock endpoint, and the backend always charged a hardcoded flat 20%
        // regardless of what the guest was shown). Defaults match that old hardcoded behavior, so
        // existing hotels see no change until an owner touches the setting.
        public bool FreeCancellationEnabled { get; set; } = true;
        public int FreeCancellationDaysBefore { get; set; } = 2;
        public CancellationFeeType CancellationFeeType { get; set; } = CancellationFeeType.Percentage;
        public decimal CancellationFeeValue { get; set; } = 20m;

        // navigation properties
        public User Owner { get; set; } = null!;
        public ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public ICollection<HotelImage> HotelImages { get; set; } = new List<HotelImage>();



    }
}
