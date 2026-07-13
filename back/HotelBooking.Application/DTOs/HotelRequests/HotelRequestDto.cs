namespace HotelBooking.Application.DTOs.HotelRequests
{
    public record HotelRequestDto
    (
        long Id,
        string Type,
        string Status,
        long OwnerId,
        string OwnerName,
        string OwnerEmail,
        long? HotelId,
        string? HotelName,
        string? City,
        string? Address,
        string? PhoneNumber,
        string? Description,
        short? Stars,
        string? DocumentDataUrl,
        string? RejectionReason,
        DateTime CreatedAt,
        DateTime? ReviewedAt
    );
}
