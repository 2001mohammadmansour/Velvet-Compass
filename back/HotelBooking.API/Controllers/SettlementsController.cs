using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Settlements;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/settlements")]
    public class SettlementsController : ControllerBase
    {
        private readonly ISettlementService _settlementService;

        public SettlementsController(ISettlementService settlementService)
            => _settlementService = settlementService;

        // Admin: per-hotel preview of what would settle right now.
        [HttpGet("preview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPreview()
            => Ok(await _settlementService.GetPreviewAsync());

        // Admin: run the settlement for one hotel.
        [HttpPost("run")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Run([FromBody] RunSettlementRequest request)
            => Ok(await _settlementService.RunAsync(User.GetUserId(), request));

        // Admin: full settlement history, optionally filtered to one hotel.
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistory([FromQuery] long? hotelId)
            => Ok(await _settlementService.GetHistoryAsync(hotelId));

        // Owner/Admin: one hotel's settlement (payout) history.
        [HttpGet("hotel/{hotelId:long}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetHotelHistory(long hotelId)
            => Ok(await _settlementService.GetHotelHistoryAsync(User.GetUserId(), User.IsInRole("Admin"), hotelId));
    }
}
