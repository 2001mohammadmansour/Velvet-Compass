namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown when an admin looks up, updates, or
    // toggles an amenity by id that doesn't exist.
    public class AmenityNotFoundException : Exception
    {
        public AmenityNotFoundException(long amenityId)
            : base($"Amenity {amenityId} was not found.") { }
    }
}
