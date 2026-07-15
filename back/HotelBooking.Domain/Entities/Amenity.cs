using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-15): please review. Admin-managed catalog of amenities/benefits.
    // Each amenity belongs to exactly one scope (Hotel or RoomType) so the hotel-benefits and
    // room-benefits picklists never mix. Amenities are never hard-deleted (see AmenityService) —
    // IsActive=false just hides them from picklists without breaking existing associations.
    public class Amenity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public AmenityScope Scope { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<HotelAmenity> HotelAmenities { get; set; } = new List<HotelAmenity>();
        public ICollection<RoomTypeAmenity> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenity>();
    }
}
