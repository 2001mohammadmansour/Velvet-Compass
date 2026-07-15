using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.DTOs.RoomTypes;

namespace HotelBooking.Application.Interfaces
{
    public interface IRoomTypeService
    {
        Task<List<RoomTypeSummaryDto>> GetRoomTypesByHotelAsync(long hotelId);
        Task<RoomTypeDetailDto> GetRoomTypeById(long hotelId, long roomTypeId);
        Task<RoomTypeDetailDto> CreateAsync(long ownerId, long HotelId, CreateRoomTypeRequest request);
        Task<RoomTypeDetailDto> UpdateAsync(long callerId, bool isAdmin, long HotelId, long roomTypeId, UpdateRoomTypeRequest request);
        Task DeleteAsync(long callerId, bool isAdmin, long HotelId, long roomTypeId);
        Task AddImageAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, AddRoomTypeImageRequest request);
        Task DeleteImageAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long imageId);

        // CHANGED BY AI (2026-07-13): please review. Backs the homepage's cross-hotel search.
        Task<List<RoomSearchResultDto>> SearchAsync();

        // CHANGED BY AI (2026-07-15): please review. New method backing the room-type-level
        // amenities checkbox grid (full-replace semantics).
        Task SetRoomTypeAmenitiesAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, List<long> amenityIds);

    }
}
