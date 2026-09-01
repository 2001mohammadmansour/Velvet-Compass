namespace HotelBooking.Application.DTOs.Booking
{
    public record CreateBookingRequest
    (
    long HotelId,
    DateOnly CheckinDate,
    DateOnly CheckoutDate,
    string? SpecialRequests,
    List<BookingItemRequest> Items,
    List<GuestRequest> Guests,
    // CHANGED BY AI (2026-07-12): please review. New field for the breakfast add-on; ignored
    // (treated as false) if the hotel doesn't have breakfast available.
    bool IncludeBreakfast = false,
    // "Online" (card, paid up front to the platform) or "CashOnArrival" (paid to the hotel at
    // the stay). Anything unrecognised — including null — is treated as CashOnArrival, matching
    // the old behaviour where a booking with no payment record was a pay-on-arrival booking.
    string? PaymentMethod = null
        );

}
