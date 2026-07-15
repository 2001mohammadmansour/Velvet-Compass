using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Pricing;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-15): please review. Seasonal-rule and occupancy-tier management is
    // hotel-scoped (moved from per-room-type) and deliberately Owner-only everywhere here (no Admin
    // role, unlike almost every other owner-editable resource in this app) — per the explicit
    // "admin not involved in pricing at all" requirement. The quote endpoint is still per room type
    // (a guest picks a room type, not a whole hotel) and is the one public action, used by the
    // guest-facing room detail page so the displayed total always matches what booking creation
    // will actually charge.
    [ApiController]
    [Route("api/v1/hotel/{hotelId:long}/pricing")]
    public class HotelPricingController : ControllerBase
    {
        private readonly IHotelPricingService _pricingService;
        private readonly IRoomPricingService _quoteService;

        public HotelPricingController(IHotelPricingService pricingService, IRoomPricingService quoteService)
        {
            _pricingService = pricingService;
            _quoteService = quoteService;
        }

        [HttpGet("/api/v1/hotel/{hotelId:long}/room-types/{roomTypeId:long}/pricing/quote")]
        public async Task<IActionResult> GetQuote(long hotelId, long roomTypeId, [FromQuery] DateOnly checkIn, [FromQuery] DateOnly checkOut)
            => Ok(await _quoteService.GetQuoteAsync(hotelId, roomTypeId, checkIn, checkOut));

        [HttpGet("seasonal-rules")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetSeasonalRules(long hotelId)
            => Ok(await _pricingService.GetSeasonalRulesAsync(User.GetUserId(), hotelId));

        [HttpPost("seasonal-rules")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateSeasonalRule(long hotelId, [FromBody] CreateSeasonalPriceRuleRequest request)
            => Ok(await _pricingService.CreateSeasonalRuleAsync(User.GetUserId(), hotelId, request));

        [HttpPut("seasonal-rules/{ruleId:long}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateSeasonalRule(long hotelId, long ruleId, [FromBody] UpdateSeasonalPriceRuleRequest request)
            => Ok(await _pricingService.UpdateSeasonalRuleAsync(User.GetUserId(), hotelId, ruleId, request));

        [HttpDelete("seasonal-rules/{ruleId:long}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteSeasonalRule(long hotelId, long ruleId)
        {
            await _pricingService.DeleteSeasonalRuleAsync(User.GetUserId(), hotelId, ruleId);
            return NoContent();
        }

        [HttpGet("occupancy-tiers")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetOccupancyTiers(long hotelId)
            => Ok(await _pricingService.GetOccupancyTiersAsync(User.GetUserId(), hotelId));

        [HttpPost("occupancy-tiers")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateOccupancyTier(long hotelId, [FromBody] CreateOccupancyPriceTierRequest request)
            => Ok(await _pricingService.CreateOccupancyTierAsync(User.GetUserId(), hotelId, request));

        [HttpPut("occupancy-tiers/{tierId:long}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateOccupancyTier(long hotelId, long tierId, [FromBody] UpdateOccupancyPriceTierRequest request)
            => Ok(await _pricingService.UpdateOccupancyTierAsync(User.GetUserId(), hotelId, tierId, request));

        [HttpDelete("occupancy-tiers/{tierId:long}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteOccupancyTier(long hotelId, long tierId)
        {
            await _pricingService.DeleteOccupancyTierAsync(User.GetUserId(), hotelId, tierId);
            return NoContent();
        }
    }
}
