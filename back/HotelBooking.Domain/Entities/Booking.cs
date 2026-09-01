using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public long UserId { get; set; }
    public long HotelId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateOnly CheckinDate { get; set; }
    public DateOnly CheckoutDate { get; set; }
    public int TotalNights { get; set; }
    public decimal TotalAmount { get; set; }

    // ─── Fee Fields ────────────────────────────────────────────
    public decimal PlatformFeeRate { get; set; } = 0.15m;
    public decimal PlatformFee { get; set; }       // 15% للمنصة
    public decimal OwnerAmount { get; set; }        // 85% للمالك

    // ─── Cancellation Fields ───────────────────────────────────
    public decimal? CancellationPenalty { get; set; }  // 20% من الإجمالي
    public decimal? RefundAmount { get; set; }          // 80% للزبون
    public DateTime? CancelledAt { get; set; }
    public string? SpecialRequests { get; set; }

    // CHANGED BY AI (2026-07-12): please review. Records whether breakfast was included and
    // what it actually cost at booking time (kept even if the hotel's price changes later).
    public bool IncludeBreakfast { get; set; } = false;
    public decimal BreakfastFee { get; set; } = 0m;

    // CHANGED BY AI (2026-07-13): please review. Records a late-modification fee (see
    // BookingService.ModifyDatesAsync) — null if the booking has never been modified, or was
    // modified for free (>=24h before check-in).
    public decimal? ModificationFee { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    public ICollection<GuestDetail> Guests { get; set; } = new List<GuestDetail>();
    public Payment? Payment { get; set; }
}
