namespace HotelBooking.Application.DTOs.Rooms
{
    public record RoomDto
    (
    long Id,
    long RoomTypeId,
    string RoomNumber,
    int? Floor,
    string? Notes,
    string Status
        );

}
