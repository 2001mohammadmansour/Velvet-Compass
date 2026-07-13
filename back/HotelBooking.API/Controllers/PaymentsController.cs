using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Payments;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
        => _paymentService = paymentService;

    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request)
        => Ok(await _paymentService.InitiateAsync(User.GetUserId(), request));

    [HttpPost("confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest request)
        => Ok(await _paymentService.ConfirmAsync(User.GetUserId(), request));

    [HttpPost("{paymentId:long}/refund")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Refund(long paymentId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _paymentService.RefundAsync(User.GetUserId(), isAdmin, paymentId));
    }
    [HttpGet("booking/{bookingId:long}")]
    [Authorize]
    public async Task<IActionResult> GetByBooking(long bookingId)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _paymentService.GetByBookingIdAsync(User.GetUserId(), isAdmin, bookingId));
    }
}
