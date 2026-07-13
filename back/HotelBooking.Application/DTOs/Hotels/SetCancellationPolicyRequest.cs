namespace HotelBooking.Application.DTOs.Hotels
{
    // CHANGED BY AI (2026-07-13): please review. New DTO for the cancellation policy settings
    // endpoint. CancellationFeeType is "Percentage" or "Flat" (case-insensitive).
    public record SetCancellationPolicyRequest(
        bool FreeCancellationEnabled,
        int FreeCancellationDaysBefore,
        string CancellationFeeType,
        decimal CancellationFeeValue
    );
}
