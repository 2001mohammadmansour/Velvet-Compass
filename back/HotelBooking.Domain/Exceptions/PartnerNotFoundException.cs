namespace HotelBooking.Domain.Exceptions
{
    public class PartnerNotFoundException : Exception
    {
        public PartnerNotFoundException(long partnerId)
            : base($"Partner {partnerId} was not found.") { }
    }
}
