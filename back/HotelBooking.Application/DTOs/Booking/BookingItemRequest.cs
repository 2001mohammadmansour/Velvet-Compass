namespace HotelBooking.Application.DTOs.Booking
{
    public record BookingItemRequest
    (
    long RoomTypeId,
    int Qty,
    // CHANGED BY AI (2026-07-15): please review. Extra beds requested for this line (0-2, capped
    // by the room type's MaxExtraBeds server-side); applies uniformly to every physical room
    // created under this line's Qty.
    int ExtraBedCount = 0
        );
}
