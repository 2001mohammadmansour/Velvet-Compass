namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-15): please review. Join entity linking a RoomType to a catalog
    // Amenity (many-to-many). Composite key (RoomTypeId, AmenityId), configured in
    // RoomTypeAmenityConfiguration.
    public class RoomTypeAmenity
    {
        public long RoomTypeId { get; set; }
        public long AmenityId { get; set; }

        public RoomType RoomType { get; set; } = null!;
        public Amenity Amenity { get; set; } = null!;
    }
}
