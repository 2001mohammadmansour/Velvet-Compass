using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Users;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;
        public UsersController(UserManager<User> userManager, AppDbContext context, IBookingService bookingService)
        {
            _userManager = userManager;
            _context = context;
            _bookingService = bookingService;
        }

        // CHANGED BY AI (2026-07-13): please review. New admin endpoint reusing the existing
        // GetMyBookingsAsync logic (already used by GET /bookings/my) for an arbitrary user, for
        // the admin user-detail drill-down.
        [HttpGet("{userId:long}/bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserBookings(long userId)
            => Ok(await _bookingService.GetMyBookingsAsync(userId));

        // CHANGED BY AI (2026-07-13): please review. Enriched with phone number, booking
        // count/amount paid to the platform, hotels owned, and suspension status — for the
        // admin user-list screen. Computed as separate simple queries aggregated in memory
        // rather than one large query, to avoid EF Core translation issues with nested
        // collections mixed with scalar aggregates in a single projection.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var bookingStats = await _context.Bookings
                .GroupBy(b => b.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    AmountPaid = g.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.PlatformFee) ?? 0
                })
                .ToListAsync();
            var bookingStatsByUser = bookingStats.ToDictionary(x => x.UserId);

            var ownedHotels = await _context.Hotels
                .Select(h => new { h.OwnerId, h.Name })
                .ToListAsync();
            var hotelsByOwner = ownedHotels
                .GroupBy(h => h.OwnerId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

            var users = await _context.Users
                .Select(u => new { u.Id, u.UserName, u.Email, u.PhoneNumber, u.Role, u.CreatedAt, u.LockoutEnabled, u.LockoutEnd })
                .ToListAsync();

            var now = DateTimeOffset.UtcNow;
            var result = users.Select(u =>
            {
                var isSuspended = u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > now;
                return new AdminUserSummaryDto(
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.Role.ToString(),
                    u.CreatedAt,
                    bookingStatsByUser.TryGetValue(u.Id, out var stats) ? stats.Count : 0,
                    bookingStatsByUser.TryGetValue(u.Id, out var stats2) ? stats2.AmountPaid : 0,
                    hotelsByOwner.TryGetValue(u.Id, out var hotels) ? hotels : new List<string>(),
                    isSuspended,
                    isSuspended ? u.LockoutEnd : null
                );
            }).ToList();

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return Ok(new
            {
                user!.Id,
                user.UserName,
                user.Email,
                user.Role,
                user.CreatedAt
            });
        }

        // CHANGED BY AI (2026-07-13): please review. New admin suspend/unsuspend actions.
        // Suspension reuses ASP.NET Identity's built-in LockoutEnd/LockoutEnabled fields (now
        // actually enforced at login — see AuthService.LoginAsync). Until = null suspends
        // indefinitely (LockoutEnd set to DateTimeOffset.MaxValue). No delete action — suspend
        // (temporary or indefinite) is the only account-removal action, by design, so no user's
        // booking/financial history is ever destroyed.
        [HttpPost("{userId:long}/suspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Suspend(long userId, [FromBody] SuspendUserRequest request)
        {
            var callerId = User.GetUserId();
            if (callerId == userId)
                throw new InvalidAdminActionException("You cannot suspend your own account.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            if (user.Role == UserRole.Admin)
                throw new InvalidAdminActionException("Admin accounts cannot be suspended from this screen.");

            var until = request.Until ?? DateTimeOffset.MaxValue;
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, until);
            return Ok();
        }

        [HttpPost("{userId:long}/unsuspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unsuspend(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            await _userManager.SetLockoutEndDateAsync(user, null);
            return Ok();
        }
    }
}
