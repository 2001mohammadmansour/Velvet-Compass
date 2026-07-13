namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-13): please review. New entity backing the Reviews feature — the
    // frontend already had a full review UI (MyBookings.js/Rooms.js/OwnerDashboard.js) but nothing
    // real behind it. One review per Booking (enforced via a unique index on BookingId), attached
    // to the Hotel and the booking's first RoomType (a Booking can span several room types via
    // BookingItems, so the first one is treated as "the" room being reviewed — same simplification
    // the frontend already made for display, e.g. roomName = items[0].roomTypeName).
    public class Review : BaseEntity
    {
        public long BookingId { get; set; }
        public long HotelId { get; set; }
        public long RoomTypeId { get; set; }
        public long GuestId { get; set; }

        public int Staff { get; set; }
        public int Location { get; set; }
        public int Facilities { get; set; }
        public int Cleanliness { get; set; }
        public int Comfort { get; set; }
        public int Value { get; set; }

        // Weighted average of the six category ratings above (cleanliness/comfort count double),
        // computed and stored server-side — never trusts a client-supplied overall score.
        public decimal OverallScore { get; set; }
        public string Comment { get; set; } = string.Empty;

        public Booking Booking { get; set; } = null!;
        public Hotel Hotel { get; set; } = null!;
        public RoomType RoomType { get; set; } = null!;
        public User Guest { get; set; } = null!;
    }
}
