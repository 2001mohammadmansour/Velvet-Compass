using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record RoomTypeImageDto
        (
        long  imageId,
        string Url,
        string? Caption,
        bool IsPrimary,
        int SortOrder
    );
    
}
