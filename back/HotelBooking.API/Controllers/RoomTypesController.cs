using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.DTOs.RoomTypes;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/hotel/{hotelId:long}/room-types")]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IReviewService _reviewService;

        public RoomTypesController(IRoomTypeService roomTypeService, IFileStorageService fileStorageService, IReviewService reviewService)
        {
            _roomTypeService = roomTypeService;
            _fileStorageService = fileStorageService;
            _reviewService = reviewService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(long hotelId, [FromQuery] DateOnly? checkIn = null, [FromQuery] DateOnly? checkOut = null)
            => Ok(await _roomTypeService.GetRoomTypesByHotelAsync(hotelId, checkIn, checkOut));

        // CHANGED BY AI (2026-07-13): please review. New public cross-hotel search endpoint,
        // replacing the frontend's previous mock "/api/rooms/search" call (which hit an endpoint
        // that never existed on this backend — the homepage search returned nothing/errored).
        // CHANGED BY AI (2026-07-15): please review. Now accepts optional checkIn/checkOut so
        // sold-out room types can be excluded and remaining stock reported (see RoomTypeServices).
        [HttpGet("~/api/v1/room-types/search")]
        public async Task<IActionResult> Search([FromQuery] DateOnly? checkIn = null, [FromQuery] DateOnly? checkOut = null)
            => Ok(await _roomTypeService.SearchAsync(checkIn, checkOut));

        [HttpGet("{roomTypeId:long}")]
        public async Task<IActionResult> GetById(long hotelId, long roomTypeId)
            => Ok(await _roomTypeService.GetRoomTypeById(hotelId, roomTypeId));

        // Owner Only
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create(long hotelId, [FromBody] CreateRoomTypeRequest request)
        {
            var result = await _roomTypeService.CreateAsync(User.GetUserId(), hotelId, request);
            return CreatedAtAction(nameof(GetById), new { hotelId, roomTypeId = result.Id }, result);
        }
        [HttpPut("{roomTypeId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Update(long hotelId, long roomTypeId, [FromBody] UpdateRoomTypeRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            return Ok(await _roomTypeService.UpdateAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, request));
        }
        [HttpDelete("{roomTypeId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(long hotelId, long roomTypeId)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomTypeService.DeleteAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId);
            return NoContent();
        }

        // CHANGED BY AI (2026-07-15): please review. New endpoint backing the owner-facing
        // room-type amenities checkbox grid (full-replace semantics).
        [HttpPatch("{roomTypeId:long}/amenities")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetAmenities(long hotelId, long roomTypeId, [FromBody] SetRoomTypeAmenitiesRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomTypeService.SetRoomTypeAmenitiesAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, request.AmenityIds);
            return Ok();
        }

        // Images
        [HttpPost("{roomTypeId:long}/images")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> AddImage(long hotelId, long roomTypeId, [FromBody] AddRoomTypeImageRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomTypeService.AddImageAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, request);
            return Ok();
        }

        [HttpDelete("{roomTypeId:long}/images/{imageId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> DeleteImage(long hotelId, long roomTypeId, long imageId)
        {
            var isAdmin = User.IsInRole("Admin");
            await _roomTypeService.DeleteImageAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, imageId);
            return NoContent();
        }

        // CHANGED BY AI (2026-07-13): please review. New real photo upload endpoint — saves the
        // file to disk via IFileStorageService, then records it the same way AddImage (above)
        // already does. Replaces the frontend's previous mock "/api/uploads/signed-urls" call.
        [HttpPost("{roomTypeId:long}/images/upload")]
        [Authorize(Roles = "Owner,Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(long hotelId, long roomTypeId, IFormFile? file, [FromForm] bool isPrimary = false, [FromForm] string? caption = null)
        {
            if (file == null || file.Length == 0)
                throw new InvalidFileUploadException("No file was uploaded.");

            var isAdmin = User.IsInRole("Admin");
            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, "room-types");
            await _roomTypeService.AddImageAsync(User.GetUserId(), isAdmin, hotelId, roomTypeId, new AddRoomTypeImageRequest(url, caption, isPrimary));
            return Ok(new { url });
        }

        // CHANGED BY AI (2026-07-13): please review. New public endpoint for the Reviews feature
        // — used by the guest-facing "Guest Reviews" modal/snippet on the room listing/reservation
        // pages.
        [HttpGet("{roomTypeId:long}/reviews")]
        public async Task<IActionResult> GetReviews(long hotelId, long roomTypeId)
            => Ok(await _reviewService.GetRoomTypeReviewsAsync(hotelId, roomTypeId));
    }
}
