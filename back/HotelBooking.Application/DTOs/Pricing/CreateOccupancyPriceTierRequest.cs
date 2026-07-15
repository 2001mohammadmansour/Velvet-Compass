namespace HotelBooking.Application.DTOs.Pricing
{
    public record CreateOccupancyPriceTierRequest(
        int MinOccupancyPercent,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
