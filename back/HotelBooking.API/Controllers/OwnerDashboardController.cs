using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Dashboard;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/owner/dashboard")]
[Authorize(Roles = "Owner,Admin")]
public class OwnerDashboardController : ControllerBase
{
    private readonly IOwnerDashboardService _dashboardService;
    public OwnerDashboardController(IOwnerDashboardService dashboardService)
        => _dashboardService = dashboardService;

    [HttpGet("{hotelId:long}")]
    public async Task<IActionResult> GetDashboard(long hotelId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _dashboardService.GetDashboardAsync(User.GetUserId(), isAdmin, hotelId));
    }

    [HttpPost("{hotelId:long}/revenue/date-range")]
    public async Task<IActionResult> GetRevenueByDateRange(long hotelId, [FromBody] DateRangeRevenueRequest request)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _dashboardService.GetRevenueByDateRangeAsync(User.GetUserId(), isAdmin, request));
    }

    [HttpPost("{hotelId:long}/track-view")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackView(long hotelId)
    {
        await _dashboardService.TrackViewAsync(hotelId);
        return Ok();
    }

    [HttpPost("{hotelId:long}/track-click")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackClick(long hotelId)
    {
        await _dashboardService.TrackClickAsync(hotelId);
        return Ok();
    }
}
