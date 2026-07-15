namespace HotelBooking.Application.DTOs.Amenities
{
    // CHANGED BY AI (2026-07-15): please review. Catalog entry for a hotel- or room-type-level
    // amenity. Scope is "Hotel" or "RoomType".
    public record AmenityDto(long Id, string Name, string? Icon, string Scope, bool IsActive);
}
