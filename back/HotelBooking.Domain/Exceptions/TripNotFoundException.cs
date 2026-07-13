namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. Thrown when a trip is looked up/edited/deleted
    // by an id that doesn't exist.
    public class TripNotFoundException : Exception
    {
        public TripNotFoundException(long tripId)
            : base($"Trip {tripId} was not found.") { }
    }
}
