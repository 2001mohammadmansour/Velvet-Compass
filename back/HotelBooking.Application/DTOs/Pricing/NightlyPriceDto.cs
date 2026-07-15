namespace HotelBooking.Application.DTOs.Pricing
{
    // CHANGED BY AI (2026-07-15): please review. Reason is a small transparency touch ("Holiday",
    // "High Demand", or null) — cheap to include since the resolver already knows which rule/tier
    // matched a given night; lets the guest UI optionally explain why a night costs more.
    public record NightlyPriceDto(
        DateOnly Date,
        decimal Price,
        string? Reason
    );
}
