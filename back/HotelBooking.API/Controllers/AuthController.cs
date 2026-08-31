using HotelBooking.API.Extensions;
using HotelBooking.Application.DTOs.Auth;
using HotelBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            // Flat AuthResponse on the normal path; a 2FA challenge when the account has 2FA on.
            return result.RequiresTwoFactor
                ? Ok(new TwoFactorRequiredResponse(true, result.ChallengeToken!))
                : Ok(result.Auth);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return NoContent();
        }

        // Self-service profile endpoints (kept on this route for the frontend). The logic lives
        // in IUserService alongside the admin user-management operations.
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
            => Ok(await _userService.GetMyProfileAsync(User.GetUserId()));

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
            => Ok(await _userService.UpdateProfileAsync(User.GetUserId(), request));

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            await _userService.ChangePasswordAsync(User.GetUserId(), request);
            return Ok(new { message = "Password changed successfully." });
        }

        // ─── 2FA ──────

        [HttpPost("2fa/setup")]
        [Authorize]
        public async Task<IActionResult> SetupTwoFactor()
            => Ok(await _authService.SetupTwoFactorAsync(User.GetUserId()));

        [HttpPost("2fa/enable")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactor([FromBody] Enable2FARequest request)
            => Ok(await _authService.EnableTwoFactorAsync(User.GetUserId(), request.Code));

        [HttpPost("2fa/disable")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactor([FromBody] Disable2FARequest request)
        {
            await _authService.DisableTwoFactorAsync(User.GetUserId(), request.Password);
            return Ok(new { message = "Two-factor authentication disabled." });
        }

        [HttpPost("2fa/verify")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] Verify2FARequest request)
            => Ok(await _authService.VerifyTwoFactorAsync(request));

        [HttpPost("2fa/recovery-codes/regenerate")]
        [Authorize]
        public async Task<IActionResult> RegenerateRecoveryCodes()
            => Ok(await _authService.RegenerateRecoveryCodesAsync(User.GetUserId()));
    }
}
