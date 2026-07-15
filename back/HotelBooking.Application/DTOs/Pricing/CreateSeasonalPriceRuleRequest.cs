namespace HotelBooking.Application.DTOs.Pricing
{
    public record CreateSeasonalPriceRuleRequest(
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
