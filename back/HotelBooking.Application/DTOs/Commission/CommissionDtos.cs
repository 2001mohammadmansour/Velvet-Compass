namespace HotelBooking.Application.DTOs.Commission
{
    // One hotel's commission position with the platform.
    public record CommissionSummaryDto(
        long HotelId,
        string HotelName,
        decimal Owed,               // 15% of what the owner kept, for finalised bookings not yet claimed
        decimal AwaitingConfirmation, // owner clicked "I paid", admin hasn't confirmed
        decimal Paid,               // lifetime confirmed
        int OwedBookingCount,
        int AwaitingBookingCount
    );

    // Admin overview across all hotels.
    public record PlatformCommissionDto(
        decimal PendingTotal,       // owed + awaiting confirmation, everywhere
        decimal CollectedTotal,     // lifetime confirmed — real platform revenue
        List<HotelCommissionRowDto> Hotels
    );

    public record HotelCommissionRowDto(
        long HotelId,
        string HotelName,
        string OwnerName,
        decimal Owed,
        decimal AwaitingConfirmation
    );
}
