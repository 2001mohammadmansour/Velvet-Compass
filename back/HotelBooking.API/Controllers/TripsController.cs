using HotelBooking.Application.DTOs.Trips;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-13): please review. New controller for the Trips feature ("Facilities
    // & Attractions" page). Reads are public; writes are Admin-only.
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;
        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _tripService.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTripRequest request)
            => Ok(await _tripService.CreateAsync(request));

        [HttpPut("{tripId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long tripId, [FromBody] UpdateTripRequest request)
            => Ok(await _tripService.UpdateAsync(tripId, request));

        [HttpDelete("{tripId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long tripId)
        {
            await _tripService.DeleteAsync(tripId);
            return NoContent();
        }
    }
}
