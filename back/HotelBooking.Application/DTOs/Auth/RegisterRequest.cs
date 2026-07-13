using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Auth
{
    public record RegisterRequest(
    string Username,
    string Email,   
    string Password,
    string Role  // "customer" أو "owner"
);

}
