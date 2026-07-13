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
        int ReviewCount
    );
}
