using HotelBooking.Application.DTOs.Common;
using HotelBooking.Application.DTOs.Hotels;

namespace HotelBooking.Application.Interfaces
{
    public interface IHotelService
    {
        Task<PagedResult<HotelSummaryDto>> GetAllAsync(HotelFilterRequest filter);
        Task<HotelDetailDto> GetByIdAsync(long hotelId);
        // CHANGED BY AI (2026-07-12): please review. Removed the ownerId parameter — it used
        // to be the caller's own id (wrong for Admin-created hotels). The target owner is now
        // resolved inside CreateHotelAsync from request.OwnerEmail instead.
        Task<HotelDetailDto> CreateHotelAsync(CreateHotelRequest request);
        Task<HotelDetailDto> UpdateHotelAsync(long callerId, bool isAdmin, long hotelId, UpdateHotelRequest request);
        Task DeleteHotelAsync(long callerId, bool isAdmin, long hotelId);
        Task AddImagesAsync(long callerId, bool isAdmin, long hotelId, string url, string? caption, bool isPrimary);
        Task DeleteImageAsync(long callerId, bool isAdmin, long hotelId, long imageId);
        Task<List<HotelSummaryDto>> GetMyHotelsAsync(long ownerId);
        // CHANGED BY AI (2026-07-12): please review. New method backing the auto-accept toggle.
        Task SetAutoAcceptBookingsAsync(long callerId, bool isAdmin, long hotelId, bool enabled);
        // CHANGED BY AI (2026-07-12): please review. New method backing the breakfast settings.
        Task SetBreakfastSettingsAsync(long callerId, bool isAdmin, long hotelId, bool available, decimal price);
        // CHANGED BY AI (2026-07-13): please review. New method backing the real cancellation
        // policy settings.
        Task SetCancellationPolicyAsync(long callerId, bool isAdmin, long hotelId, bool freeCancellationEnabled, int freeCancellationDaysBefore, string cancellationFeeType, decimal cancellationFeeValue);
        //Task<HotelDetailDto> GetMyHotelDetailsAsync(long ownerId);
        // CHANGED BY AI (2026-07-15): please review. New method backing the hotel-level amenities
        // checkbox grid (full-replace semantics).
        Task SetHotelAmenitiesAsync(long callerId, bool isAdmin, long hotelId, List<long> amenityIds);
    }
}
