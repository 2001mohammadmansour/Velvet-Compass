namespace HotelBooking.Application.DTOs.Rooms
{
    public record UpdateRoomRequest
    (
    string RoomNumber,
    int? Floor,
    string? Notes,
    string Status
        );

}
