using HotelBooking.Application.DTOs.Pricing;

namespace HotelBooking.Application.Interfaces
{
    // CHANGED BY AI (2026-07-15): please review. The pricing resolution/quote engine — separate
    // from IHotelPricingService (which only manages the CRUD rows). Used by both the public
    // guest-facing quote endpoint and BookingService (so the displayed price and the charged price
    // can never drift apart, since they share this exact same code path).
    public interface IRoomPricingService
    {
        // CHANGED BY AI (2026-07-15): please review. hotelId now drives seasonal rule / occupancy
        // tier lookup (hotel-scoped); roomTypeId is still needed for the per-room-type occupancy
        // pool (with a hotel-wide fallback for small pools — see RoomPricingService).
        Task<List<NightlyPriceDto>> GetNightlyPricesAsync(long roomId, long hotelId, long roomTypeId, decimal basePrice, DateOnly checkinDate, DateOnly checkoutDate);

        // CHANGED BY AI (2026-07-15): please review. Public guest-facing quote — no specific
        // physical room is chosen yet at browse time, so the (dormant) per-room override tier is
        // naturally skipped (internally resolved with a sentinel room id that can never match a
        // real RoomAvailability row); seasonal + occupancy still apply normally.
        Task<PriceQuoteDto> GetQuoteAsync(long hotelId, long roomTypeId, DateOnly checkinDate, DateOnly checkoutDate);
    }
}
