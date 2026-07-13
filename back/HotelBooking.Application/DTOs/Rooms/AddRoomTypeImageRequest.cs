namespace HotelBooking.Application.DTOs.Rooms
{
    public record AddRoomTypeImageRequest(
        string Url,
        string? Caption,
        bool IsPrimary
    );
}
