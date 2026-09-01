using HotelBooking.Application.DTOs.Settlements;

namespace HotelBooking.Application.Interfaces
{
    public interface ISettlementService
    {
        // Admin: per-hotel preview of everything that would settle right now.
        Task<List<SettlementPreviewDto>> GetPreviewAsync();

        // Admin: run the settlement for one hotel — creates a Settlement record and stamps its
        // matured bookings as settled.
        Task<SettlementDto> RunAsync(long adminUserId, RunSettlementRequest request);

        // Admin: full settlement history (optionally filtered to one hotel).
        Task<List<SettlementDto>> GetHistoryAsync(long? hotelId);

        // Owner/Admin: one hotel's settlement history (payout history for the owner).
        Task<List<SettlementDto>> GetHotelHistoryAsync(long callerId, bool isAdmin, long hotelId);
    }
}
