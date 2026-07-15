namespace HotelBooking.Application.DTOs.Pricing
{
    public record SeasonalPriceRuleDto(
        long Id,
        long HotelId,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
