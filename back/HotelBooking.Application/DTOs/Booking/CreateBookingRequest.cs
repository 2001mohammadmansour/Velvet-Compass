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
    bool IncludeBreakfast = false
        );

}
