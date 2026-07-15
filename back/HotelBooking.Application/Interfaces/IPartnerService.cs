using HotelBooking.Application.DTOs.Partners;

namespace HotelBooking.Application.Interfaces
{
    public interface IPartnerService
    {
        Task<List<PartnerDto>> GetAllAsync();
        Task<PartnerDto> CreateAsync(CreatePartnerRequest request);
        Task<PartnerDto> UpdateAsync(long partnerId, UpdatePartnerRequest request);
        Task DeleteAsync(long partnerId);
        Task<PartnerDto> SetImageAsync(long partnerId, string imageUrl);
    }
}
