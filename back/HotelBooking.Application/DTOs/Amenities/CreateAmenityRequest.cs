namespace HotelBooking.Application.DTOs.Amenities
{
    // CHANGED BY AI (2026-07-15): please review. Admin-only: creates a new catalog amenity. Scope
    // must be "Hotel" or "RoomType".
    public record CreateAmenityRequest(string Name, string? Icon, string Scope);
}
