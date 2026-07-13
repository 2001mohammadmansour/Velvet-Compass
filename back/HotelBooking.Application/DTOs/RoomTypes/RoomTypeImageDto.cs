namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record RoomTypeImageDto
    (
        long Id,
        string Url,
        string? Caption,
        bool IsPrimary,
        int SortOrder
        );
}
