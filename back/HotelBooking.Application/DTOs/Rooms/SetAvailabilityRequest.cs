namespace HotelBooking.Application.DTOs.Rooms
{
    public record SetAvailabilityRequest
    (
    DateOnly From,
    DateOnly To,
    string Status,
    decimal? PriceOverride
        );

}
