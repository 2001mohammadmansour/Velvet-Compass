namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record RoomTypeDetailDto
    (
    long Id,
    long HotelId,
    string Name,
    string Description,
    int Capacity,
    int Beds,
    decimal BasePrice,
    DateTime CreatedAt,
    List<RoomTypeImageDto> Images

        );
}
