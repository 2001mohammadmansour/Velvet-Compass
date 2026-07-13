namespace HotelBooking.Application.DTOs.Booking
{
    // CHANGED BY AI (2026-07-13): please review. New DTO for the modify-booking-dates endpoint.
    public record ModifyBookingDatesRequest(DateOnly CheckinDate, DateOnly CheckoutDate);
}
