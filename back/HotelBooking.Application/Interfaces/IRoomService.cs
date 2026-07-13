using HotelBooking.Application.DTOs.Rooms;

namespace HotelBooking.Application.Interfaces
{
    public interface IRoomService
    {
        //Task<List<RoomDto>> GetAllByHotelAsync(long hotelId);
        Task<List<RoomDto>> GetAllByHotelAndTypeAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId);
        Task<List<RoomDto>> GetAllByHotelAsync(long callerId, bool isAdmin, long hotelId);
        Task<RoomDto> CreateAsync(long ownerId, long hotelId, long roomTypeId, CreateRoomRequest request);
        Task<RoomDto> UpdateAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId, UpdateRoomRequest request);
        Task DeleteAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId);
        Task<List<RoomAvailabilityDto>> GetAvailabilityAsync(long roomId, DateOnly from, DateOnly to);
        Task SetAvailabilityAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId, SetAvailabilityRequest request);

    }
}
