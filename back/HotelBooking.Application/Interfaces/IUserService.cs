using HotelBooking.Application.DTOs.Auth;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.DTOs.Users;

namespace HotelBooking.Application.Interfaces
{
    // All operations on a user record: the caller's own profile (self-service) and the admin
    // user-management screens. Auth/token/session concerns stay in IAuthService.
    public interface IUserService
    {
        // Self-service ("my account")
        Task<UserProfileDto> GetMyProfileAsync(long userId);
        Task<UserProfileDto> UpdateProfileAsync(long userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(long userId, ChangePasswordRequest request);

        // Admin
        Task<List<AdminUserSummaryDto>> GetAllAsync();
        Task<List<BookingSummaryDto>> GetUserBookingsAsync(long userId);
        Task SuspendAsync(long callerId, long targetUserId, DateTimeOffset? until);
        Task UnsuspendAsync(long targetUserId);
    }
}
