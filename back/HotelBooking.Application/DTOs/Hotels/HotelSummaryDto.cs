using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record HotelSummaryDto
    (
        long HotelId,
        string Name,
        string City,
        string Country,
        int StarRating,
        string? PrimaryImageUrl
        );
}
