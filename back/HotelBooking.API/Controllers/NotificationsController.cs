using HotelBooking.API.Extensions;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-13): please review. New controller for the Notifications feature.
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
            => Ok(await _notificationService.GetMyNotificationsAsync(User.GetUserId()));

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
            => Ok(new { count = await _notificationService.GetUnreadCountAsync(User.GetUserId()) });

        [HttpPost("{notificationId:long}/read")]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            await _notificationService.MarkAsReadAsync(User.GetUserId(), notificationId);
            return Ok();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(User.GetUserId());
            return Ok();
        }
    }
}
