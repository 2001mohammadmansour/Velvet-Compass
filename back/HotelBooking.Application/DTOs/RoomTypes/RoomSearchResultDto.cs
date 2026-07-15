namespace HotelBooking.Application.DTOs.RoomTypes
{
    // CHANGED BY AI (2026-07-13): please review. New DTO backing the homepage's cross-hotel
    // search (previously called a mock "/api/rooms/search" endpoint that never existed on this
    // backend). Flattens a room type + its hotel into one row, matching how the frontend already
    // displays search results. No date-based availability filtering yet (checkIn/checkOut are
    // accepted by the frontend only for display/carry-through to booking, same limitation the
    // single-hotel room listing already has).
    public record RoomSearchResultDto(
        long Id,
        long HotelId,
        string Name,
        string HotelName,
        string City,
        string Country,
        int HotelStars,
        decimal Price,
        int Capacity,
        string? PrimaryImageUrl,
        decimal? AvgScore,
        int ReviewCount,
        // CHANGED BY AI (2026-07-15): please review. Description + extra-bed scalar fields, cheap
        // to include here (no join needed). Amenities (a collection) deliberately excluded from
        // this cross-hotel, potentially-many-rows DTO to avoid an N+1/payload-bloat risk — see
        // RoomTypeSummaryDto (single-hotel, low row count) for where amenities are exposed instead.
        string Description = "",
        bool AllowExtraBed = false,
        int MaxExtraBeds = 0,
        string ExtraBedPriceType = "Percentage",
        decimal ExtraBedPriceForOneBed = 0m,
        decimal ExtraBedPriceForTwoBeds = 0m
    );
}
