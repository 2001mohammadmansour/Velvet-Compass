namespace HotelBooking.Application.DTOs.Rooms
{
    public record CreateRoomRequest
    (
    string RoomNumber,
    int? Floor,
    string? Notes
        );

}
