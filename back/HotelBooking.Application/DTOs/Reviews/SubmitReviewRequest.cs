namespace HotelBooking.Application.DTOs.Reviews
{
    public record SubmitReviewRequest(
        int Staff,
        int Location,
        int Facilities,
        int Cleanliness,
        int Comfort,
        int Value,
        string Comment
    );
}
