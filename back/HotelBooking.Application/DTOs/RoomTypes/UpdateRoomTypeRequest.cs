namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record UpdateRoomTypeRequest
    (
    string Name,
    string Description,
    int Capacity,
    int Beds,
    decimal BasePrice
        );

}
