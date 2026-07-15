using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    public class RoomType : BaseEntity
    {
        public long HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Beds { get; set; }
        public decimal BasePrice { get; set; }

        // CHANGED BY AI (2026-07-15): please review. New extra-bed system: owners can allow up to
        // 2 extra beds per room type, priced either as a percentage of the nightly room price or a
        // flat dollar amount per night. Defaults (false/0/Percentage/0/0) mean existing room types
        // opt out until an owner configures them via the dashboard.
        public bool AllowExtraBed { get; set; } = false;
        public int MaxExtraBeds { get; set; } = 0;
        public ExtraBedPriceType ExtraBedPriceType { get; set; } = ExtraBedPriceType.Percentage;
        public decimal ExtraBedPriceForOneBed { get; set; } = 0m;
        public decimal ExtraBedPriceForTwoBeds { get; set; } = 0m;

        public Hotel Hotel { get; set; } = null!;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<RoomTypeImage> RoomTypeImages { get; set; } = new List<RoomTypeImage>();
        // CHANGED BY AI (2026-07-15): please review. New join for the room-type-level amenities
        // catalog (sea view, minibar, balcony, etc.) — see Amenity/RoomTypeAmenity.
        public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}
