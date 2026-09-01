using HotelBooking.Application.DTOs.Commission;

namespace HotelBooking.Application.Interfaces
{
    public interface ICommissionService
    {
        // Owner/Admin: one hotel's commission position.
        Task<CommissionSummaryDto> GetForHotelAsync(long callerId, bool isAdmin, long hotelId);

        // Owner (or admin on their behalf): "I've paid my outstanding commission for this hotel."
        // Snapshots the amount on every finalised, unclaimed booking.
        Task<CommissionSummaryDto> ClaimAsync(long callerId, bool isAdmin, long hotelId);

        // Admin: confirm the money actually arrived — moves it into platform revenue.
        Task<CommissionSummaryDto> ConfirmAsync(long adminId, long hotelId);

        // Admin: platform-wide pending vs collected, per hotel.
        Task<PlatformCommissionDto> GetPlatformOverviewAsync();
    }
}
