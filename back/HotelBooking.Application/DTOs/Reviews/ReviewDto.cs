namespace HotelBooking.Application.DTOs.Reviews
{
    public record ReviewDto(
        long Id,
        string RoomName,
        string GuestName,
        decimal OverallScore,
        string Comment,
        DateTime CreatedAt
    );

    public record CategoryAveragesDto(
        decimal Staff,
        decimal Location,
        decimal Facilities,
        decimal Cleanliness,
        decimal Comfort,
        decimal Value
    );

    public record ReviewSummaryDto(
        decimal AvgScore,
        int ReviewCount,
        CategoryAveragesDto? CategoryAverages,
        List<ReviewDto> Reviews
    );

    // CHANGED BY AI (2026-07-13): please review. Full single-review detail for the admin
    // moderation view (click a review in the Users tab to see everything about it).
    public record ReviewDetailDto(
        long Id,
        long BookingId,
        string HotelName,
        string RoomName,
        string GuestName,
        int Staff,
        int Location,
        int Facilities,
        int Cleanliness,
        int Comfort,
        int Value,
        decimal OverallScore,
        string Comment,
        DateTime CreatedAt
    );
}
