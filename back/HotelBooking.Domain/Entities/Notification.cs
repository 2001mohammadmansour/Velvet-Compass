using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-13): please review. New entity backing in-app notifications. Title/
    // Message are pre-rendered plain text at creation time (not templated at read time), so
    // there's nothing to re-render later even if the related booking/hotel/hotel-request later
    // changes. RelatedBookingId/RelatedHotelRequestId are plain ids (not real FKs) purely so the
    // frontend can deep-link — deliberately not modeled as navigation properties to keep this
    // entity simple and independent of those aggregates' lifecycle.
    public class Notification : BaseEntity
    {
        public long UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long? RelatedBookingId { get; set; }
        public long? RelatedHotelRequestId { get; set; }
        public bool IsRead { get; set; } = false;

        public User User { get; set; } = null!;
    }
}
