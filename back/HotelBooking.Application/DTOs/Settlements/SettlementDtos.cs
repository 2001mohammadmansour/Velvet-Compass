namespace HotelBooking.Application.DTOs.Settlements
{
    // What would be settled for one hotel right now: every booking whose stay has finished and
    // that hasn't been settled yet.
    public record SettlementPreviewDto(
        long HotelId,
        string HotelName,
        string OwnerName,
        int BookingCount,
        decimal OwnerCredit,          // owed to the owner — 85% of matured online bookings
        decimal PlatformCommission,   // owed to the platform — 15% of matured cash bookings
        decimal ClawbackAmount,       // deducted from OwnerCredit — refunds of already-settled bookings
        decimal NetAmount,            // net of the three — the single transfer
        string Direction              // "PlatformToOwner" | "OwnerToPlatform"
    );

    public record RunSettlementRequest(long HotelId, string? PeriodLabel);

    public record SettlementDto(
        long Id,
        long HotelId,
        string HotelName,
        string PeriodLabel,
        string Direction,
        decimal OwnerCredit,
        decimal PlatformCommission,
        decimal NetAmount,
        int BookingCount,
        System.DateTime CreatedAt
    );
}
