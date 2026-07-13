using HotelBooking.Application.DTOs.Booking;
using HotelBooking.Application.DTOs.Bookings;

namespace HotelBooking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(long userId, CreateBookingRequest request);
    Task<BookingDto> GetByIdAsync(long callerId, bool isAdmin, long bookingId);
    Task<List<BookingSummaryDto>> GetMyBookingsAsync(long userId);
    Task<List<BookingSummaryDto>> GetHotelBookingsAsync(long callerId, bool isAdmin, long hotelId);
    Task CancelAsync(long userId, long bookingId);
    // CHANGED BY AI (2026-07-12): please review. New owner-facing accept/reject actions for
    // Pending bookings (previously there was no way for an owner to act on a booking at all).
    Task<BookingDto> AcceptAsync(long callerId, bool isAdmin, long bookingId);
    Task<BookingDto> RejectAsync(long callerId, bool isAdmin, long bookingId);
    // CHANGED BY AI (2026-07-13): please review. New guest-facing "modify booking dates" action.
    Task<BookingDto> ModifyDatesAsync(long userId, long bookingId, ModifyBookingDatesRequest request);
}
