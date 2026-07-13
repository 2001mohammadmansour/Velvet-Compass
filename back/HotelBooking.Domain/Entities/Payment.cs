using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Method { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
        public string? TransactionRef { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }
}
