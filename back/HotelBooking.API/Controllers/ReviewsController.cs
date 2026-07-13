using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-13): please review. New controller for admin review moderation —
    // viewing a review's full detail and deleting it (e.g. if it's toxic/abusive). Submitting a
    // review stays on BookingsController; the read summaries stay nested under
    // Hotels/RoomTypes — this controller is only for acting on one review by its own id.
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("{reviewId:long}")]
        public async Task<IActionResult> GetById(long reviewId)
            => Ok(await _reviewService.GetByIdAsync(reviewId));

        [HttpDelete("{reviewId:long}")]
        public async Task<IActionResult> Delete(long reviewId)
        {
            await _reviewService.DeleteAsync(reviewId);
            return NoContent();
        }
    }
}
