namespace HotelBooking.Domain.Enum
{
    // CHANGED BY AI (2026-07-13): please review. New enum for the Notifications feature.
    public enum NotificationType
    {
        BookingConfirmed,
        BookingRejected,
        BookingCancelled,
        BookingModified,
        NewBooking,
        HotelRequestApproved,
        HotelRequestRejected,
        NewReview,
        // ─── Admin-facing ────────────────────────────────────────
        HotelRequestSubmitted,  // an owner submitted a create/edit request for review
        CommissionClaimed,      // an owner marked a commission payment as sent
        NewOwner                // a new hotel-owner account registered
    }
}
