namespace HotelBooking.Application.DTOs.Amenities
{
    // CHANGED BY AI (2026-07-15): please review. Admin-only: updates an existing amenity's name,
    // icon, and active state. Scope is intentionally not editable post-creation — changing it after
    // hotels/room types already reference it via two different join tables would be inconsistent.
    public record UpdateAmenityRequest(string Name, string? Icon, bool IsActive);
}
