namespace HotelBooking.Application.DTOs.Pricing
{
    public record UpdateSeasonalPriceRuleRequest(
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
