using HotelBooking.Application.DTOs.Notifications;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-13): please review. New service backing in-app notifications.
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(long userId, string type, string title, string message, long? relatedBookingId = null, long? relatedHotelRequestId = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = Enum.Parse<NotificationType>(type, ignoreCase: true),
                Title = title,
                Message = message,
                RelatedBookingId = relatedBookingId,
                RelatedHotelRequestId = relatedHotelRequestId,
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(long userId, int take = 30)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationDto(
                    n.Id, n.Type.ToString(), n.Title, n.Message,
                    n.RelatedBookingId, n.RelatedHotelRequestId, n.IsRead, n.CreatedAt))
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(long userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(long userId, long notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification == null) return;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(long userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
    }
}
