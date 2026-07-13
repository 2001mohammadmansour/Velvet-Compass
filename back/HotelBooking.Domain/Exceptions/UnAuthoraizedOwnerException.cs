using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Domain.Exceptions
{
    public class UnAuthoraizedOwnerException : Exception
    {
        public UnAuthoraizedOwnerException()
            :base("You are not authorized to perform this action on this hotel.") {}
    }
}
