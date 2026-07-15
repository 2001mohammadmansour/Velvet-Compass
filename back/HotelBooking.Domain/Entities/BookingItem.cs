namespace HotelBooking.Domain.Entities
{
    public class BookingItem : BaseEntity
    {
        public long BookingId { get; set; }
        public long RoomTypeId { get; set; }
        public long? RoomId { get; set; }
        public int Nights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public int Qty { get; set; }

        // CHANGED BY AI (2026-07-15): please review. Extra-bed count chosen for this line at
        // booking time (0-2, fixed for the life of the booking — never resubmitted on modify-dates,
        // see BookingService.ModifyDatesAsync) and the resulting fee (per night x nights, folded
        // into TotalPrice).
        public int ExtraBedCount { get; set; } = 0;
        public decimal ExtraBedFee { get; set; } = 0m;

        // Navigation
        public Booking Booking { get; set; } = null!;
        public RoomType RoomType { get; set; } = null!;
        public Room? Room { get; set; }
    }
}
