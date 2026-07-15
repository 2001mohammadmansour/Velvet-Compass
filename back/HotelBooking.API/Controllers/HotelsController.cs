using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IOwnerDashboardService _ownerDashboardService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IReviewService _reviewService;

        public HotelsController(IHotelService hotelService, IOwnerDashboardService ownerDashboardService, IFileStorageService fileStorageService, IReviewService reviewService)
        {
            _hotelService = hotelService;
            _ownerDashboardService = ownerDashboardService;
            _fileStorageService = fileStorageService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] HotelFilterRequest filter)
        {
            var result = await _hotelService.GetAllAsync(filter);
            return Ok(result);
        }
        [HttpGet("{hotelId:long}")]
        public async Task<IActionResult> GetById(long hotelId)
        {
            var result = await _hotelService.GetByIdAsync(hotelId);
            await _ownerDashboardService.TrackViewAsync(hotelId); // ← تتبع تلقائي
            return Ok(result);
        }
        // ********* Owner //
        [HttpGet("my")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMyHotels()
        {
            var ownerId = User.GetUserId();
            var hotels = await _hotelService.GetMyHotelsAsync(ownerId);
            return Ok(hotels);
        }
        // CHANGED BY AI (2026-07-12): please review. This used to assign the hotel to the
        // logged-in Admin (via User.GetUserId()) instead of the intended pending owner, and
        // would always fail its role check as a result. The target owner now comes from
        // request.OwnerEmail, resolved inside CreateHotelAsync.
        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateHotelRequest request)
        {
            var result = await _hotelService.CreateHotelAsync(request);
            return CreatedAtAction(nameof(GetById),
                new { hotelId = result.HotelId },
                result
                );
        }
        [HttpPut("{hotelId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Update(long hotelId, [FromBody] UpdateHotelRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _hotelService.UpdateHotelAsync(User.GetUserId(), isAdmin, hotelId, request);
            return Ok(result);
        }
        // CHANGED BY AI (2026-07-12): please review. New endpoint backing the owner-facing
        // "auto-accept bookings" toggle.
        [HttpPatch("{hotelId:long}/auto-accept-bookings")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetAutoAcceptBookings(long hotelId, [FromBody] SetAutoAcceptBookingsRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.SetAutoAcceptBookingsAsync(User.GetUserId(), isAdmin, hotelId, request.Enabled);
            return Ok();
        }
        // CHANGED BY AI (2026-07-12): please review. New endpoint backing the owner-facing
        // breakfast add-on toggle/price.
        [HttpPatch("{hotelId:long}/breakfast-settings")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetBreakfastSettings(long hotelId, [FromBody] SetBreakfastSettingsRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.SetBreakfastSettingsAsync(User.GetUserId(), isAdmin, hotelId, request.Available, request.Price);
            return Ok();
        }
        // CHANGED BY AI (2026-07-13): please review. New endpoint backing the real cancellation
        // policy settings (replaces the old mock "/api/owner/{hotelId}/cancel-policy" call).
        [HttpPatch("{hotelId:long}/cancellation-policy")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetCancellationPolicy(long hotelId, [FromBody] SetCancellationPolicyRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.SetCancellationPolicyAsync(User.GetUserId(), isAdmin, hotelId, request.FreeCancellationEnabled, request.FreeCancellationDaysBefore, request.CancellationFeeType, request.CancellationFeeValue);
            return Ok();
        }
        // CHANGED BY AI (2026-07-15): please review. New endpoint backing the owner-facing hotel
        // amenities checkbox grid (full-replace semantics).
        [HttpPatch("{hotelId:long}/amenities")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> SetAmenities(long hotelId, [FromBody] SetHotelAmenitiesRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.SetHotelAmenitiesAsync(User.GetUserId(), isAdmin, hotelId, request.AmenityIds);
            return Ok();
        }
        [HttpDelete("{hoetlId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(long hotelId)
        {
            var isAdmin = User.IsInRole("Admin");

            var result = _hotelService.DeleteHotelAsync(User.GetUserId(), isAdmin, hotelId);
            return NoContent();
        }
        // Image *****

        [HttpPost("{hotelId:long}/images")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> AddImage(long hotelId, [FromBody] AddImageRequest request)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.AddImagesAsync(User.GetUserId(), isAdmin, hotelId, request.Url, request.Caption, request.Isprimary);
            return Ok();
        }

        [HttpDelete("{hotelId:long}/images/{imageId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> DeleteImage(long hotelId, long imageId)
        {
            var isAdmin = User.IsInRole("Admin");
            await _hotelService.DeleteImageAsync(User.GetUserId(), isAdmin, hotelId, imageId);
            return NoContent();
        }

        // CHANGED BY AI (2026-07-13): please review. New real photo upload endpoint — saves the
        // file to disk via IFileStorageService, then records it the same way AddImage (above)
        // already does. Replaces the frontend's previous mock "/api/uploads/signed-urls" call.
        [HttpPost("{hotelId:long}/images/upload")]
        [Authorize(Roles = "Owner,Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(long hotelId, IFormFile? file, [FromForm] bool isPrimary = false, [FromForm] string? caption = null)
        {
            if (file == null || file.Length == 0)
                throw new InvalidFileUploadException("No file was uploaded.");

            var isAdmin = User.IsInRole("Admin");
            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, "hotels");
            await _hotelService.AddImagesAsync(User.GetUserId(), isAdmin, hotelId, url, caption ?? "", isPrimary);
            return Ok(new { url });
        }

        // CHANGED BY AI (2026-07-13): please review. New public endpoint for the Reviews feature
        // — used by the owner dashboard's "Guest Reviews" section.
        [HttpGet("{hotelId:long}/reviews")]
        public async Task<IActionResult> GetReviews(long hotelId)
            => Ok(await _reviewService.GetHotelReviewsAsync(hotelId));
    }
}
