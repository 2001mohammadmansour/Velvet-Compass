using HotelBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Domain.Exceptions
{
    public class HotelNotFoundException : Exception
    {
        public HotelNotFoundException(long hotelId)
            : base($"Hotel with id {hotelId} was not found.") { }
    }
}
