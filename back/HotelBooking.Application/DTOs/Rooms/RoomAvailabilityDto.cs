namespace HotelBooking.Application.DTOs.Rooms
{
    public record RoomAvailabilityDto
    (
    DateOnly Date,
    string Status,
    decimal? PriceOverride
        );

}
