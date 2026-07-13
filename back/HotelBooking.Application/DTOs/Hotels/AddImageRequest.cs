using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Application.DTOs.Hotels
{
    public record AddImageRequest
        (
            string Url,
            string Caption,
            bool Isprimary
        );
}
