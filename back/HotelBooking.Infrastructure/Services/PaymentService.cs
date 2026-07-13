using HotelBooking.Application.DTOs.Payments;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context) => _context = context;

        public async Task<PaymentDto> InitiateAsync(long userId, InitiatePaymentRequest request)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == userId)
                ?? throw new BookingNotFoundException(request.BookingId);

            if (booking.Status == BookingStatus.Cancelled)
                throw new Exception("Cannot pay for a cancelled booking.");

            if (booking.Payment != null)
                throw new Exception("Payment already exists for this booking.");

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalAmount,
                Currency = request.Currency,
                Method = request.Method,
                Status = PaymentStatus.Initiated
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return MapToDto(payment);
        }

        public async Task<PaymentDto> ConfirmAsync(long userId, ConfirmPaymentRequest request)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.Booking.UserId == userId)
                ?? throw new Exception("Payment not found.");

            if (payment.Status != PaymentStatus.Initiated)
                throw new Exception("Payment cannot be confirmed in its current state.");

            payment.Status = PaymentStatus.Paid;
            payment.TransactionRef = request.TransactionRef;
            payment.PaidAt = DateTime.UtcNow;

            payment.Booking.Status = BookingStatus.Confirmed;

            await _context.SaveChangesAsync();
            return MapToDto(payment);
        }

        public async Task<PaymentDto> RefundAsync(long callerId, bool isAdmin, long paymentId)
        {
            {
                var payment = await _context.Payments
                    .Include(p => p.Booking).ThenInclude(b => b.Hotel)
                    .FirstOrDefaultAsync(p => p.Id == paymentId)
                    ?? throw new Exception("Payment not found.");

                var isHotelOwner = payment.Booking.Hotel.OwnerId == callerId;
                if (!isHotelOwner && !isAdmin)
                    throw new UnAuthoraizedOwnerException();

                if (payment.Status != PaymentStatus.Paid)
                    throw new Exception("Only paid payments can be refunded.");

                payment.Status = PaymentStatus.Refunded;
                payment.Booking.Status = BookingStatus.Cancelled;

                await _context.SaveChangesAsync();
                return MapToDto(payment);
            }
        }

        public async Task<PaymentDto> GetByBookingIdAsync(long callerId, bool isAdmin, long bookingId)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking).ThenInclude(b => b.Hotel)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId)
                ?? throw new Exception("Payment not found.");

            var isBookingOwner = payment.Booking.UserId == callerId;
            var isHotelOwner = payment.Booking.Hotel.OwnerId == callerId;

            if (!isBookingOwner && !isHotelOwner && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            return MapToDto(payment);
        }

        private static PaymentDto MapToDto(Payment p) => new(
            p.Id, p.BookingId, p.Amount, p.Currency,
            p.Method, p.Status.ToString(), p.TransactionRef,
            p.PaidAt, p.CreatedAt
        );
    }
}

