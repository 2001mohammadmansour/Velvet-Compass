namespace HotelBooking.Application.DTOs.Booking
{
    public record GuestRequest
    (
    string FullName,
    string? PassportNo,
    string? Nationality,
    DateOnly? DateOfBirth,
    bool IsPrimary
        );

}
