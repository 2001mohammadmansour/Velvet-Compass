using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record HotelDetailDto
    (

    long HotelId,
    string Name,
    string Description,
    string Address,
    string City,
    string Country,
    short StarRating,
    string Phone,
    string Email,
    DateTime CreatedAt,
    List<RoomTypeImageDto> Images,
    // CHANGED BY AI (2026-07-12): please review. Exposes the auto-accept toggle to the frontend.
    bool AutoAcceptBookings,
    // CHANGED BY AI (2026-07-12): please review. Exposes the breakfast add-on toggle/price.
    bool BreakfastAvailable,
    decimal BreakfastPrice,
    // CHANGED BY AI (2026-07-13): please review. Exposes the real per-hotel cancellation policy.
    bool FreeCancellationEnabled,
    int FreeCancellationDaysBefore,
    string CancellationFeeType,
    decimal CancellationFeeValue

        );
    
}
