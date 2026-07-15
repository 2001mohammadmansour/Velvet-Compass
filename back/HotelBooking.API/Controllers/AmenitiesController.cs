using HotelBooking.Application.DTOs.Amenities;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-15): please review. New controller for the amenities catalog.
    // Reading by scope is public (used to populate the hotel/room-type picklists on both the
    // owner dashboard and guest-facing pages); catalog management is Admin-only, EXCEPT creating
    // new amenities, which Owners can also do (shared catalog, per explicit request — if the
    // admin-curated list is missing something a hotel needs, the owner can add it themselves and
    // it becomes available to every owner going forward, same as an admin-added one). Editing,
    // deactivating, and reactivating stay Admin-only to keep catalog curation centralized.
    [ApiController]
    [Route("api/v1/amenities")]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByScope([FromQuery] string scope)
            => Ok(await _amenityService.GetByScopeAsync(scope));

        // CHANGED BY AI (2026-07-15): please review. Admin-only catalog view — includes inactive
        // amenities so there's a way to find and reactivate one.
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllByScope([FromQuery] string scope)
            => Ok(await _amenityService.GetAllByScopeAsync(scope));

        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateAmenityRequest request)
            => Ok(await _amenityService.CreateAsync(request));

        [HttpPut("{amenityId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long amenityId, [FromBody] UpdateAmenityRequest request)
            => Ok(await _amenityService.UpdateAsync(amenityId, request));

        [HttpPatch("{amenityId:long}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(long amenityId)
        {
            await _amenityService.DeactivateAsync(amenityId);
            return Ok();
        }

        [HttpPatch("{amenityId:long}/reactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate(long amenityId)
        {
            await _amenityService.ReactivateAsync(amenityId);
            return Ok();
        }
    }
}
