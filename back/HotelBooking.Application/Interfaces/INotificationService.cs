using HotelBooking.Application.DTOs.Notifications;

namespace HotelBooking.Application.Interfaces
{
    public interface INotificationService
    {
        // Called as a side effect of existing actions (booking accept/reject/cancel/create/
        // modify, hotel request approve/reject, review submit) — not a standalone workflow.
        // CHANGED BY AI (2026-07-13): please review. `type` is a string (not the NotificationType
        // enum) because HotelBooking.Application doesn't reference HotelBooking.Domain (matching
        // this project's existing convention — see SubmitHotelRequestRequest's string Type — enum
        // parsing happens in the Infrastructure-layer implementation instead).
        Task CreateAsync(long userId, string type, string title, string message, long? relatedBookingId = null, long? relatedHotelRequestId = null);
        Task<List<NotificationDto>> GetMyNotificationsAsync(long userId, int take = 30);
        Task<int> GetUnreadCountAsync(long userId);
        Task MarkAsReadAsync(long userId, long notificationId);
        Task MarkAllAsReadAsync(long userId);
    }
}
