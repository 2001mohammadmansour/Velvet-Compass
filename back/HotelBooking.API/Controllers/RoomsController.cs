using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/hotel/{hotelId:long}/room-types/{roomTypeId:long}/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
            => _roomService = roomService;

        //Owner only 

        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetAll(long hotelId, long roomTypeId)
        {
            var isAdmin = User.IsInRole("Admin");
            return Ok(await _roomService.GetAllByHotelAndTypeAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId));
        }

        [HttpGet("~/api/v1/hotels/{hotelId:long}/rooms")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetAllByHotel(long hotelId)
        {
            var isAdmin = User.IsInRole("Admin");
            return Ok(await _roomService.GetAllByHotelAsync(User.GetUserId(), isAdmin, hotelId));
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create(long hotelId, long roomTypeId, [FromBody] CreateRoomRequest request)
        {
            var result = await _roomService.CreateAsync(User.GetUserId(), hotelId, roomTypeId, request);
            return CreatedAtAction(nameof(GetAll), new { hotelId, roomTypeId }, result);
        }

        [HttpPut("{roomId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Update(long hotelId, long roomTypeId, long roomId, [FromBody] UpdateRoomRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            return Ok(await _roomService.UpdateAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, roomId, request));
        }

        [HttpDelete("{roomId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(long hotelId, long roomTypeId, long roomId)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomService.DeleteAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, roomId);
            return NoContent();
        }

        // ─── Availability ──────────────────────────────────────────

        [HttpGet("{roomId:long}/availability")]
        public async Task<IActionResult> GetAvailability(
            long roomId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
            => Ok(await _roomService.GetAvailabilityAsync(roomId, from, to));

        [HttpPost("{roomId:long}/availability")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetAvailability(
            long hotelId, long roomTypeId, long roomId,
            [FromBody] SetAvailabilityRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomService.SetAvailabilityAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, roomId, request);
            return Ok();
        }
    }
}
