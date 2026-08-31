using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Users;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Admin user-detail drill-down: reuses the same "my bookings" logic for an arbitrary user.
        [HttpGet("{userId:long}/bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserBookings(long userId)
            => Ok(await _userService.GetUserBookingsAsync(userId));

        // Admin user-list screen.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => Ok(await _userService.GetAllAsync());

        // Admin suspend/unsuspend. until == null suspends indefinitely. No delete action by
        // design, so no user's booking/financial history is ever destroyed.
        [HttpPost("{userId:long}/suspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Suspend(long userId, [FromBody] SuspendUserRequest request)
        {
            await _userService.SuspendAsync(User.GetUserId(), userId, request.Until);
            return Ok();
        }

        [HttpPost("{userId:long}/unsuspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unsuspend(long userId)
        {
            await _userService.UnsuspendAsync(userId);
            return Ok();
        }
    }
}
