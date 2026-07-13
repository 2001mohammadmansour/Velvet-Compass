using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record HotelFilterRequest(
        string? City,
        string? Country,
        int? MinStars,
        int Page = 1,
        int PageSize = 10


    );
}
