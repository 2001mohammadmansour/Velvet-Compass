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
    int ReviewCount = 0
        );


}
