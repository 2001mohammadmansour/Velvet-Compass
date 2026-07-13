using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record UpdateHotelRequest
    (
            string Name,
            string Description,
            string Address,
            string City,
            string Country,
            short StarRating,
            string Phone,
            string Email
        );
}
