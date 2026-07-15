namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-15): please review. Thrown when a booking's guest count exceeds the
    // effective capacity (room capacity + any extra beds requested) of the room(s) being booked.
    public class GuestCountExceedsCapacityException : Exception
    {
        public GuestCountExceedsCapacityException(int guestCount, int effectiveCapacity)
            : base($"{guestCount} guests exceeds this room's capacity of {effectiveCapacity} (including any extra beds selected).") { }
    }
}
