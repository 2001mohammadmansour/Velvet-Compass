namespace HotelBooking.Domain.Enum
{
    // How the guest pays for the booking — decides who collects the money and therefore
    // the direction of the monthly settlement between owner and platform.
    public enum PaymentMethod
    {
        // Guest pays the platform up front (card). Platform holds the full amount and owes the
        // owner their 85% at settlement.
        Online,

        // Guest pays the hotel directly in cash at the stay. Owner holds the full amount and
        // owes the platform its 15% commission at settlement.
        CashOnArrival
    }
}
