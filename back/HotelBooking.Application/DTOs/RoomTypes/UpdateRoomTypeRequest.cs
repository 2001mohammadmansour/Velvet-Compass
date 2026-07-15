namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record UpdateRoomTypeRequest
    (
    string Name,
    string Description,
    int Capacity,
    int Beds,
    decimal BasePrice,
    // CHANGED BY AI (2026-07-15): please review. New extra-bed system fields.
    bool AllowExtraBed = false,
    int MaxExtraBeds = 0,
    string ExtraBedPriceType = "Percentage",
    decimal ExtraBedPriceForOneBed = 0m,
    decimal ExtraBedPriceForTwoBeds = 0m
        );

}
