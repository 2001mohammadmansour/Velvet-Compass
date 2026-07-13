namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record CreateRoomTypeRequest
    (
    string Name,
    string Description,
    int Capacity,
    int Beds,
    decimal BasePrice

        );

}
