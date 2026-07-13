using HotelBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Domain.Exceptions
{
    public class OwnerNotFoundException : Exception
    {
        public OwnerNotFoundException(long ownerId)
            :base($"Owner with id {ownerId} was not found.") { }

        // CHANGED BY AI (2026-07-12): please review. Added this overload so hotel creation
        // can report a missing owner when looked up by email instead of by id.
        public OwnerNotFoundException(string ownerEmail)
            :base($"Owner with email {ownerEmail} was not found, or is not registered as an Owner.") { }
    }
}
