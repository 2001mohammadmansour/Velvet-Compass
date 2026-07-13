namespace HotelBooking.Application.DTOs.HotelRequests
{
    // Type: "Create" or "Edit" (case-insensitive). HotelId is only used/required for "Edit".
    public record SubmitHotelRequestRequest
    (
        string Type,
        long? HotelId,
        string? HotelName,
        string? City,
        string? Address,
        string? PhoneNumber,
        string? Description,
        short? Stars,
        string? DocumentDataUrl
    );
}
