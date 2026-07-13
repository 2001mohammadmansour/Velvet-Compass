using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Auth
{
    public record LoginRequest
        (
        string Email,
        string Password
        );
    
    
}
