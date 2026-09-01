namespace HotelBooking.Domain.Enum
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed,

        // Guest never checked in. Set by the owner/admin; excluded from settlement the same way
        // a cancellation is, but keeps the booking on record.
        NoShow
    }
}
