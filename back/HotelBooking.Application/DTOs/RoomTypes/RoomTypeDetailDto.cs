using HotelBooking.Application.DTOs.Amenities;

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
    List<RoomTypeImageDto> Images,
    // CHANGED BY AI (2026-07-15): please review. Room-type-level amenities (sea view, minibar,
    // balcony, etc.) and the extra-bed system settings.
    List<AmenityDto> Amenities,
    bool AllowExtraBed,
    int MaxExtraBeds,
    string ExtraBedPriceType,
    decimal ExtraBedPriceForOneBed,
    decimal ExtraBedPriceForTwoBeds

        );
}
