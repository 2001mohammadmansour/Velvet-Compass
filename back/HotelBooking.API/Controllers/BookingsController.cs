using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Booking;
using HotelBooking.Application.DTOs.Reviews;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IReviewService _reviewService;

    public BookingsController(IBookingService bookingService, IReviewService reviewService)
    {
        _bookingService = bookingService;
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var result = await _bookingService.CreateAsync(User.GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { bookingId = result.Id }, result);
    }

    [HttpGet("{bookingId:long}")]
    [Authorize]
    public async Task<IActionResult> GetById(long bookingId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _bookingService.GetByIdAsync(User.GetUserId(), isAdmin, bookingId));
    }
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
        => Ok(await _bookingService.GetMyBookingsAsync(User.GetUserId()));

    [HttpGet("hotel/{hotelId:long}")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> GetHotelBookings(long hotelId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _bookingService.GetHotelBookingsAsync(User.GetUserId(), isAdmin, hotelId));
    }
    [HttpPost("{bookingId:long}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(long bookingId)
    {
        await _bookingService.CancelAsync(User.GetUserId(), bookingId);
        return Ok(new { message = "Booking cancelled successfully." });
    }

    // CHANGED BY AI (2026-07-12): please review. New owner-facing accept/reject actions.
    [HttpPost("{bookingId:long}/accept")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Accept(long bookingId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _bookingService.AcceptAsync(User.GetUserId(), isAdmin, bookingId));
    }

    [HttpPost("{bookingId:long}/reject")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Reject(long bookingId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _bookingService.RejectAsync(User.GetUserId(), isAdmin, bookingId));
    }

    // CHANGED BY AI (2026-07-13): please review. New endpoint for the Reviews feature. Not
    // role-restricted, matching Create/Cancel/GetById above (whoever the booking belongs to can
    // act on it — SubmitAsync already scopes the lookup to booking.UserId == caller).
    [HttpPost("{bookingId:long}/review")]
    [Authorize]
    public async Task<IActionResult> SubmitReview(long bookingId, [FromBody] SubmitReviewRequest request)
    {
        var result = await _reviewService.SubmitAsync(User.GetUserId(), bookingId, request);
        return Ok(result);
    }

    // CHANGED BY AI (2026-07-13): please review. New guest-facing "modify booking dates" action.
    [HttpPatch("{bookingId:long}/dates")]
    [Authorize]
    public async Task<IActionResult> ModifyDates(long bookingId, [FromBody] ModifyBookingDatesRequest request)
    {
        var result = await _bookingService.ModifyDatesAsync(User.GetUserId(), bookingId, request);
        return Ok(result);
    }
}
