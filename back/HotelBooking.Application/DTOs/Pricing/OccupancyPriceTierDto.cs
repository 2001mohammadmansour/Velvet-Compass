namespace HotelBooking.Application.DTOs.Pricing
{
    public record OccupancyPriceTierDto(
        long Id,
        long HotelId,
        int MinOccupancyPercent,
        string AdjustmentType,
        decimal AdjustmentValue
    );
}
