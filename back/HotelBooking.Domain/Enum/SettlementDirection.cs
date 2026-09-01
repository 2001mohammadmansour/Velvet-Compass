namespace HotelBooking.Domain.Enum
{
    public enum SettlementDirection
    {
        // Platform pays the owner (net of online bookings the platform collected).
        PlatformToOwner,

        // Owner pays the platform (net of cash bookings the owner collected).
        OwnerToPlatform
    }
}
