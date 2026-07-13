using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
        => _dashboardService = dashboardService;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
        => Ok(await _dashboardService.GetDashboardAsync());

    [HttpGet("revenue/date-range")]
    public async Task<IActionResult> GetRevenueByDateRange(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to)
        => Ok(await _dashboardService.GetPlatformRevenueByDateRangeAsync(from, to));
}
