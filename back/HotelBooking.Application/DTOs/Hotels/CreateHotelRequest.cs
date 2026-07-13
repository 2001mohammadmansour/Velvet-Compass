using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    // CHANGED BY AI (2026-07-12): please review. Added OwnerEmail so the Create endpoint
    // can target a specific pending owner instead of assuming the caller (Admin) is the owner.
    public record CreateHotelRequest
        (
         string OwnerEmail,
         string Name ,
         string Description ,
         string Address ,
         string City ,
         string Country ,
         short StarRating,
         string Phone,
         string Email
        );
}
