using HotelBooking.Application.DTOs.Auth;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.DTOs.Users;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services.Users
{
    // Everything about a user record. Self-service profile methods used to live in AuthService;
    // the admin list/suspend logic used to live directly in UsersController against AppDbContext.
    // Both are consolidated here.
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;

        public UserService(UserManager<User> userManager, AppDbContext context, IBookingService bookingService)
        {
            _userManager = userManager;
            _context = context;
            _bookingService = bookingService;
        }

        // ─── Self-service ──────────────────────────────────────────────────────────

        public async Task<UserProfileDto> GetMyProfileAsync(long userId)
        {
            var user = await FindUserOrThrow(userId);
            return MapToProfileDto(user);
        }

        // Email is intentionally never accepted/changed here.
        public async Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateProfileRequest request)
        {
            var user = await FindUserOrThrow(userId);

            var desiredUsername = (request.Username ?? "").Trim();
            if (string.IsNullOrWhiteSpace(desiredUsername))
                throw new InvalidRequestException("Username can't be empty.");

            if (!string.Equals(user.UserName, desiredUsername, StringComparison.Ordinal))
            {
                var existing = await _userManager.FindByNameAsync(desiredUsername);
                if (existing is not null && existing.Id != user.Id)
                    throw new InvalidRequestException("That username is already taken.");

                var setNameResult = await _userManager.SetUserNameAsync(user, desiredUsername);
                if (!setNameResult.Succeeded)
                    throw new InvalidRequestException(string.Join(", ", setNameResult.Errors.Select(e => e.Description)));
            }

            user.PhoneNumber = NormalizePhone(request.PhoneNumber);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

            return MapToProfileDto(user);
        }

        public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request)
        {
            var user = await FindUserOrThrow(userId);

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                throw new InvalidRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // ─── Admin ────────────────────────────────────────────────────────────────

        public Task<List<BookingSummaryDto>> GetUserBookingsAsync(long userId)
            => _bookingService.GetMyBookingsAsync(userId);

        // Enriched with phone, booking count / amount paid to the platform, hotels owned, and
        // suspension status — for the admin user-list screen. Computed as separate simple
        // queries aggregated in memory rather than one large query, to avoid EF Core
        // translation issues with nested collections mixed with scalar aggregates.
        public async Task<List<AdminUserSummaryDto>> GetAllAsync()
        {
            // Only count bookings whose stay has actually happened — matches how revenue is
            // recognised everywhere else (a booking made for a future stay isn't earned yet).
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var bookingStats = await _context.Bookings
                .GroupBy(b => b.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    AmountPaid = g.Where(b =>
                            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                            && b.CheckoutDate < today)
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

            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            return users.Select(u =>
            {
                var isSuspended = u.IsSuspended;
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
        }

        // Suspension reuses ASP.NET Identity's LockoutEnd/LockoutEnabled fields (enforced at
        // login via UserManager.IsLockedOutAsync). until == null suspends indefinitely.
        // No delete action by design — booking/financial history is never destroyed.
        public async Task SuspendAsync(long callerId, long targetUserId, DateTimeOffset? until)
        {
            if (callerId == targetUserId)
                throw new InvalidAdminActionException("You cannot suspend your own account.");

            var user = await FindUserOrThrow(targetUserId);

            if (user.Role == UserRole.Admin)
                throw new InvalidAdminActionException("Admin accounts cannot be suspended from this screen.");

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, until ?? User.IndefiniteSuspensionUtc);
        }

        public async Task UnsuspendAsync(long targetUserId)
        {
            var user = await FindUserOrThrow(targetUserId);
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private async Task<User> FindUserOrThrow(long userId)
            => await _userManager.FindByIdAsync(userId.ToString())
               ?? throw new UserNotFoundException(userId);

        private static string? NormalizePhone(string? phone)
            => string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

        private static UserProfileDto MapToProfileDto(User user) => new(
            user.Id, user.UserName!, user.Email!, user.PhoneNumber, user.Role.ToString(), user.CreatedAt, user.TwoFactorEnabled
        );
    }
}
