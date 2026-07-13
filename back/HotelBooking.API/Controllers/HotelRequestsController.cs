using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.HotelRequests;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/hotel-requests")]
    [Authorize]
    public class HotelRequestsController : ControllerBase
    {
        private readonly IHotelRequestService _service;
        public HotelRequestsController(IHotelRequestService service) => _service = service;

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Submit([FromBody] SubmitHotelRequestRequest request)
        {
            var ownerId = User.GetUserId();
            return Ok(await _service.SubmitAsync(ownerId, request));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMy()
        {
            var ownerId = User.GetUserId();
            return Ok(await _service.GetMyRequestsAsync(ownerId));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpPost("{id:long}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(long id)
            => Ok(await _service.ApproveAsync(id));

        [HttpPost("{id:long}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(long id, [FromBody] RejectHotelRequestRequest request)
            => Ok(await _service.RejectAsync(id, request.Reason));
    }
}
