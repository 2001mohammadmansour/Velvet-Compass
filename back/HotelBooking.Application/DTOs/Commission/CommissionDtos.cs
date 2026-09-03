namespace HotelBooking.Application.DTOs.Commission
{
    // Sent by the owner when they claim a payment — their own Sham Cash wallet/name, so the admin
    // can match it against their app before confirming.
    public record ClaimCommissionRequest(string? SenderWallet, string? SenderName);

    // One hotel's commission position with the platform.
    public record CommissionSummaryDto(
        long HotelId,
        string HotelName,
        decimal Owed,               // 15% of what the owner kept, for finalised bookings not yet claimed
        decimal AwaitingConfirmation, // owner clicked "I paid", admin hasn't confirmed
        decimal Paid,               // lifetime confirmed
        int OwedBookingCount,
        int AwaitingBookingCount,
        string? SenderWallet = null, // who the awaiting payment was sent from (if any)
        string? SenderName = null
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
        decimal AwaitingConfirmation,
        string? SenderWallet = null,
        string? SenderName = null,
        List<CommissionBookingLineDto>? Lines = null
    );

    // One unpaid booking behind a hotel's commission total — shown when the admin
    // expands a row to see exactly which bookings make it up.
    public record CommissionBookingLineDto(
        long BookingId,
        DateOnly CheckinDate,
        DateOnly CheckoutDate,
        string Basis,          // "stay" (completed) or "cancellation" (penalty)
        decimal KeptAmount,    // what the owner kept — the 15% base
        decimal Commission,    // 15% of KeptAmount
        string State           // "owed" or "awaiting"
    );
}
