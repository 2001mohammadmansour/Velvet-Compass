namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-15): please review. Join entity linking a Hotel to a catalog Amenity
    // (many-to-many). Composite key (HotelId, AmenityId), configured in HotelAmenityConfiguration.
    public class HotelAmenity
    {
        public long HotelId { get; set; }
        public long AmenityId { get; set; }

        public Hotel Hotel { get; set; } = null!;
        public Amenity Amenity { get; set; } = null!;
    }
}
