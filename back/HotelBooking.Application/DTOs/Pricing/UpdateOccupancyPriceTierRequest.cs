namespace HotelBooking.Application.DTOs.Pricing
{
    public record UpdateOccupancyPriceTierRequest(
        int MinOccupancyPercent,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
