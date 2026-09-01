using HotelBooking.API.Extensions;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/commission")]
    public class CommissionController : ControllerBase
    {
        private readonly ICommissionService _commissionService;

        public CommissionController(ICommissionService commissionService)
            => _commissionService = commissionService;

        // Owner/Admin: one hotel's commission position.
        [HttpGet("hotel/{hotelId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetForHotel(long hotelId)
            => Ok(await _commissionService.GetForHotelAsync(User.GetUserId(), User.IsInRole("Admin"), hotelId));

        // Owner: "I've paid my outstanding commission for this hotel."
        [HttpPost("hotel/{hotelId:long}/claim")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Claim(long hotelId)
            => Ok(await _commissionService.ClaimAsync(User.GetUserId(), User.IsInRole("Admin"), hotelId));

        // Admin: confirm the payment arrived.
        [HttpPost("hotel/{hotelId:long}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(long hotelId)
            => Ok(await _commissionService.ConfirmAsync(User.GetUserId(), hotelId));

        // Admin: platform-wide pending vs collected.
        [HttpGet("overview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Overview()
            => Ok(await _commissionService.GetPlatformOverviewAsync());
    }
}
