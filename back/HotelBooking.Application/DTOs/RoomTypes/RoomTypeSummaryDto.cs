using HotelBooking.Application.DTOs.Amenities;

namespace HotelBooking.Application.DTOs.RoomTypes
{
    public record RoomTypeSummaryDto
    (
    long Id,
    string Name,
    int Capacity,
    int Beds,
    decimal BasePrice,
    string? PrimaryImageUrl,
    // CHANGED BY AI (2026-07-13): please review. New fields for the Reviews feature.
    decimal? AvgScore = null,
    int ReviewCount = 0,
    // CHANGED BY AI (2026-07-15): please review. Description was already collected on the backend
    // but never surfaced here; now added alongside amenities and the extra-bed settings so guest
    // list views (Rooms.js) can show them without a second detail fetch.
    string Description = "",
    List<AmenityDto>? Amenities = null,
    bool AllowExtraBed = false,
    int MaxExtraBeds = 0,
    string ExtraBedPriceType = "Percentage",
    decimal ExtraBedPriceForOneBed = 0m,
    decimal ExtraBedPriceForTwoBeds = 0m
        );


}
