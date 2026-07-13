using HotelBooking.Application.DTOs.HotelRequests;

namespace HotelBooking.Application.Interfaces
{
    public interface IHotelRequestService
    {
        Task<HotelRequestDto> SubmitAsync(long ownerId, SubmitHotelRequestRequest request);
        Task<List<HotelRequestDto>> GetMyRequestsAsync(long ownerId);
        Task<List<HotelRequestDto>> GetAllAsync();
        Task<HotelRequestDto> ApproveAsync(long requestId);
        Task<HotelRequestDto> RejectAsync(long requestId, string reason);
    }
}
