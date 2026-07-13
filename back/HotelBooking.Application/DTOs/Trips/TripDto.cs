namespace HotelBooking.Application.DTOs.Trips
{
    public record TripDto(
        long Id,
        string Title,
        string City,
        decimal Price,
        string PriceLabel,
        string Type,
        string Duration,
        string Difficulty,
        string Description
    );
}
