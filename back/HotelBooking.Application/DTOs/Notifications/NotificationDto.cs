namespace HotelBooking.Application.DTOs.Notifications
{
    public record NotificationDto(
        long Id,
        string Type,
        string Title,
        string Message,
        long? RelatedBookingId,
        long? RelatedHotelRequestId,
        bool IsRead,
        DateTime CreatedAt
    );
}
