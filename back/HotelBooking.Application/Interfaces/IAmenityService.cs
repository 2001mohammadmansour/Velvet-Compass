using HotelBooking.Application.DTOs.Amenities;

namespace HotelBooking.Application.Interfaces
{
    // CHANGED BY AI (2026-07-15): please review. Admin-managed amenity catalog CRUD, plus a
    // public/authenticated list-by-scope used to populate the hotel/room-type picklists.
    public interface IAmenityService
    {
        Task<List<AmenityDto>> GetByScopeAsync(string scope);
        // CHANGED BY AI (2026-07-15): please review. Admin-only: includes inactive amenities too,
        // so the catalog management screen can find something to reactivate. GetByScopeAsync stays
        // active-only since it backs the public/owner picklists.
        Task<List<AmenityDto>> GetAllByScopeAsync(string scope);
        Task<AmenityDto> CreateAsync(CreateAmenityRequest request);
        Task<AmenityDto> UpdateAsync(long amenityId, UpdateAmenityRequest request);
        Task DeactivateAsync(long amenityId);
        Task ReactivateAsync(long amenityId);
    }
}
