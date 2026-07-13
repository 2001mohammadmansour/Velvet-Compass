namespace HotelBooking.Domain.Entities
{
    public class GuestDetail : BaseEntity
    {
        public long BookingId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PassportNo { get; set; }
        public string? Nationality { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public bool IsPrimary { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }
}
