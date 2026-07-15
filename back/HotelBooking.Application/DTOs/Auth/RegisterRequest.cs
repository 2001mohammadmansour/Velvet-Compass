using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Auth
{
    public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string Role,  // "customer" أو "owner"
    // CHANGED BY AI (2026-07-13): please review. Was already collected by the sign-up form but
    // silently dropped before reaching the backend — now actually persisted (see AuthService).
    string? PhoneNumber = null
);

}
