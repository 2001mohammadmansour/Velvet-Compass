using HotelBooking.Application.DTOs.Booking;
using HotelBooking.Application.DTOs.Bookings;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        const decimal platformFeeRate = 0.15m;

        public BookingService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public async Task<BookingDto> CreateAsync(long userId, CreateBookingRequest request)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == request.HotelId)
                ?? throw new HotelNotFoundException(request.HotelId);

            if (request.CheckinDate >= request.CheckoutDate)
                throw new Exception("Checkout date must be after checkin date.");

            var totalNights = request.CheckoutDate.DayNumber - request.CheckinDate.DayNumber;

            var bookingItems = new List<BookingItem>();
            decimal totalAmount = 0;

            foreach (var itemRequest in request.Items)
            {
                var roomType = await _context.RoomTypes
                    .FirstOrDefaultAsync(rt => rt.Id == itemRequest.RoomTypeId && rt.HotelId == request.HotelId)
                    ?? throw new RoomTypeNotFoundException(itemRequest.RoomTypeId);

                var availableRooms = await GetAvailableRoomsAsync(itemRequest.RoomTypeId, request.CheckinDate, request.CheckoutDate, itemRequest.Qty);
                if (availableRooms.Count < itemRequest.Qty)
                    throw new RoomNotAvailableException();

                var pricePerNight = await GetEffectivePriceAsync(
                    availableRooms[0].Id, request.CheckinDate, roomType.BasePrice);

                var itemTotal = pricePerNight * totalNights * itemRequest.Qty;
                totalAmount += itemTotal;

                for (int i = 0; i < itemRequest.Qty; i++)
                {
                    bookingItems.Add(new BookingItem
                    {
                        RoomTypeId = itemRequest.RoomTypeId,
                        RoomId = availableRooms[i].Id,
                        Nights = totalNights,
                        PricePerNight = pricePerNight,
                        TotalPrice = pricePerNight * totalNights,
                        Qty = 1
                    });
                }
            }

            var guests = request.Guests.Select(g => new GuestDetail
            {
                FullName = g.FullName,
                PassportNo = g.PassportNo,
                Nationality = g.Nationality,
                DateOfBirth = g.DateOfBirth,
                IsPrimary = g.IsPrimary
            }).ToList();

            // CHANGED BY AI (2026-07-12): please review. New breakfast add-on pricing: only
            // applies if the hotel actually offers it, priced per guest per night.
            var includeBreakfast = request.IncludeBreakfast && hotel.BreakfastAvailable;
            var breakfastFee = includeBreakfast
                ? Math.Round(hotel.BreakfastPrice * totalNights * request.Guests.Count, 2)
                : 0m;
            totalAmount += breakfastFee;

            // CHANGED BY AI (2026-07-12): please review. New bookings now start Confirmed
            // instead of Pending when the hotel has auto-accept enabled.
            var booking = new Booking
            {
                UserId = userId,
                HotelId = request.HotelId,
                CheckinDate = request.CheckinDate,
                CheckoutDate = request.CheckoutDate,
                TotalNights = totalNights,
                TotalAmount = totalAmount,
                PlatformFeeRate = platformFeeRate,
                PlatformFee = Math.Round(totalAmount * platformFeeRate, 2),
                OwnerAmount = Math.Round(totalAmount * (1 - platformFeeRate), 2),
                SpecialRequests = request.SpecialRequests,
                Status = hotel.AutoAcceptBookings ? BookingStatus.Confirmed : BookingStatus.Pending,
                Items = bookingItems,
                Guests = guests,
                IncludeBreakfast = includeBreakfast,
                BreakfastFee = breakfastFee
            };

            _context.Bookings.Add(booking);

            foreach (var item in bookingItems)
            {
                if (item.RoomId.HasValue)
                    await BlockRoomDatesAsync(item.RoomId.Value, request.CheckinDate, request.CheckoutDate);
            }

            // CHANGED BY AI (2026-07-13): please review. Fixes a race condition: two concurrent
            // bookings for the last available room could both pass the availability check above
            // before either commits, since that check and this save aren't atomic. The database
            // already has a unique index on RoomAvailabilities(RoomId, Date) (see
            // RoomAvailabilityConfiguration), so the second writer's insert now fails here at the
            // database level instead of silently succeeding — this just turns that failure into a
            // clean, expected error instead of an unhandled crash.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new RoomNotAvailableException();
            }

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature — lets the owner know a new booking came in without having to keep
            // refreshing the dashboard.
            await _notificationService.CreateAsync(
                hotel.OwnerId, NotificationType.NewBooking.ToString(),
                "New booking received",
                $"A new {booking.Status} booking was made for {hotel.Name} ({request.CheckinDate:yyyy-MM-dd} → {request.CheckoutDate:yyyy-MM-dd}).",
                relatedBookingId: booking.Id);

            return await GetByIdAsync(userId, false, booking.Id);

        }
        public async Task<BookingDto> GetByIdAsync(long callerId, bool isAdmin, long bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Items).ThenInclude(i => i.RoomType)
                .Include(b => b.Guests)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            var isBookingOwner = booking.UserId == callerId;
            var isHotelOwner = booking.Hotel.OwnerId == callerId;

            if (!isBookingOwner && !isHotelOwner && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            // CHANGED BY AI (2026-07-13): please review. Fills in the real Reviewable/HasReview
            // values (MapToDto can't do this itself — it's a plain sync mapper with no DB access).
            var hasReview = await _context.Reviews.AnyAsync(r => r.BookingId == bookingId);
            var reviewable = !hasReview
                && booking.Status != BookingStatus.Cancelled
                && booking.CheckoutDate < DateOnly.FromDateTime(DateTime.UtcNow);

            return MapToDto(booking) with { Reviewable = reviewable, HasReview = hasReview };
        }
        public async Task<List<BookingSummaryDto>> GetHotelBookingsAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            return await _context.Bookings
                .Include(b => b.Hotel)
                .Where(b => b.HotelId == hotelId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingSummaryDto(
                    b.Id, b.Hotel.Name, b.Status.ToString(),
                    b.CheckinDate, b.CheckoutDate, b.TotalAmount, b.CreatedAt,
                    _context.Reviews.Any(r => r.BookingId == b.Id),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => (decimal?)r.OverallScore).FirstOrDefault(),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => r.Comment).FirstOrDefault(),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => (long?)r.Id).FirstOrDefault()))
                .ToListAsync();
        }
        public async Task<List<BookingSummaryDto>> GetMyBookingsAsync(long userId)
        {
            // CHANGED BY AI (2026-07-13): please review. Added review fields (see
            // BookingSummaryDto) for the Reviews feature — used by the admin user-detail
            // drill-down, which previously showed a permanent "not available yet" placeholder.
            return await _context.Bookings
                .Include(b => b.Hotel)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingSummaryDto(
                    b.Id, b.Hotel.Name, b.Status.ToString(),
                    b.CheckinDate, b.CheckoutDate, b.TotalAmount, b.CreatedAt,
                    _context.Reviews.Any(r => r.BookingId == b.Id),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => (decimal?)r.OverallScore).FirstOrDefault(),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => r.Comment).FirstOrDefault(),
                    _context.Reviews.Where(r => r.BookingId == b.Id).Select(r => (long?)r.Id).FirstOrDefault()))
                .ToListAsync();
        }
        public async Task CancelAsync(long userId, long bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Items)
                .Include(b => b.Payment)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Status == BookingStatus.Cancelled)
                throw new Exception("Booking is already cancelled.");

            if (booking.Status == BookingStatus.Completed)
                throw new Exception("Cannot cancel a completed booking.");

            var penalty = ComputeCancellationPenalty(booking.Hotel, booking.CheckinDate, booking.TotalAmount);
            var refund = Math.Round(booking.TotalAmount - penalty, 2);

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationPenalty = penalty;
            booking.RefundAmount = refund;

            // حرّر الغرف
            foreach (var item in booking.Items.Where(i => i.RoomId.HasValue))
            {
                var availability = await _context.RoomAvailabilities
                    .Where(a => a.RoomId == item.RoomId &&
                                a.Date >= booking.CheckinDate &&
                                a.Date < booking.CheckoutDate)
                    .ToListAsync();

                availability.ForEach(a => a.Status = RoomAvailabilityStatus.Free);
            }

            // Partial (or full) refund per the hotel's cancellation policy
            if (booking.Payment?.Status == PaymentStatus.Paid)
            {
                booking.Payment.Status = PaymentStatus.Refunded;
                booking.Payment.TransactionRef = $"REFUND-{booking.Payment.TransactionRef}";
            }

            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature — lets the owner know a guest cancelled without having to keep refreshing.
            await _notificationService.CreateAsync(
                booking.Hotel.OwnerId, NotificationType.BookingCancelled.ToString(),
                "Booking cancelled",
                $"A guest cancelled their booking at {booking.Hotel.Name} ({booking.CheckinDate:yyyy-MM-dd} → {booking.CheckoutDate:yyyy-MM-dd}).",
                relatedBookingId: booking.Id);
        }

        // CHANGED BY AI (2026-07-13): please review. Replaces the old hardcoded flat 20% penalty
        // with the hotel's real, owner-configured cancellation policy: free if cancelling at
        // least FreeCancellationDaysBefore days before check-in (when enabled), otherwise a
        // percentage or flat fee, whichever the owner picked. Capped at the booking total so the
        // refund can never go negative. Shared by CancelAsync and ModifyDatesAsync's late-fee
        // rule doesn't reuse this — that's a distinct, separately-defined rule (see
        // ModifyDatesAsync) — this one is specifically the cancellation policy.
        private static decimal ComputeCancellationPenalty(Hotel hotel, DateOnly checkinDate, decimal totalAmount)
        {
            var daysUntilCheckin = (checkinDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalDays;
            var isFree = hotel.FreeCancellationEnabled && daysUntilCheckin >= hotel.FreeCancellationDaysBefore;
            if (isFree) return 0m;

            var penalty = hotel.CancellationFeeType == CancellationFeeType.Percentage
                ? Math.Round(totalAmount * (hotel.CancellationFeeValue / 100m), 2)
                : hotel.CancellationFeeValue;

            return Math.Min(Math.Round(penalty, 2), totalAmount);
        }
        // CHANGED BY AI (2026-07-12): please review. New owner-facing accept/reject actions.
        // Only valid on Pending bookings — Confirmed/Cancelled/Completed bookings can't be
        // accepted or rejected. (In practice this only applies to "pay on arrival" bookings,
        // since paid bookings are confirmed automatically via PaymentService.)
        public async Task<BookingDto> AcceptAsync(long callerId, bool isAdmin, long bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            if (booking.Status != BookingStatus.Pending)
                throw new Exception("Only pending bookings can be accepted.");

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature.
            await _notificationService.CreateAsync(
                booking.UserId, NotificationType.BookingConfirmed.ToString(),
                "Booking confirmed",
                $"Your booking at {booking.Hotel.Name} ({booking.CheckinDate:yyyy-MM-dd} → {booking.CheckoutDate:yyyy-MM-dd}) has been confirmed.",
                relatedBookingId: booking.Id);

            return await GetByIdAsync(callerId, isAdmin, bookingId);
        }
        public async Task<BookingDto> RejectAsync(long callerId, bool isAdmin, long bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            if (booking.Status != BookingStatus.Pending)
                throw new Exception("Only pending bookings can be rejected.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;

            foreach (var item in booking.Items.Where(i => i.RoomId.HasValue))
            {
                var availability = await _context.RoomAvailabilities
                    .Where(a => a.RoomId == item.RoomId &&
                                a.Date >= booking.CheckinDate &&
                                a.Date < booking.CheckoutDate)
                    .ToListAsync();

                availability.ForEach(a => a.Status = RoomAvailabilityStatus.Free);
            }

            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature.
            await _notificationService.CreateAsync(
                booking.UserId, NotificationType.BookingRejected.ToString(),
                "Booking rejected",
                $"Your booking request at {booking.Hotel.Name} ({booking.CheckinDate:yyyy-MM-dd} → {booking.CheckoutDate:yyyy-MM-dd}) was rejected by the hotel.",
                relatedBookingId: booking.Id);

            return await GetByIdAsync(callerId, isAdmin, bookingId);
        }

        // CHANGED BY AI (2026-07-13): please review. New guest-facing "modify booking dates"
        // action. Reassigns each item to an available room of the same room type for the new
        // dates (not necessarily the same physical room — same reasoning as CreateAsync), frees
        // the old date range, recomputes pricing for the new night count, and re-applies the
        // hotel's auto-accept rule if the booking was Confirmed (mirrors CreateAsync's own
        // Confirmed-vs-Pending logic).
        public async Task<BookingDto> ModifyDatesAsync(long userId, long bookingId, ModifyBookingDatesRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.Items).ThenInclude(i => i.RoomType)
                .Include(b => b.Guests)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new BookingNotModifiableException("Cancelled or completed bookings can't be modified.");

            if (booking.CheckinDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BookingNotModifiableException("This booking's check-in date has already passed.");

            if (request.CheckinDate >= request.CheckoutDate)
                throw new Exception("Checkout date must be after checkin date.");

            // CHANGED BY AI (2026-07-13): please review. Late-modification fee: free if modifying
            // at least 24 hours before check-in; otherwise the guest forfeits the OLD booking's
            // full amount (1-night stays) or just the first night's price (multi-night stays).
            // This is a distinct rule from the hotel's cancellation policy
            // (ComputeCancellationPenalty) — modifying isn't cancelling.
            var hoursUntilCheckin = (booking.CheckinDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).TotalHours;
            var modificationFee = 0m;
            if (hoursUntilCheckin < 24)
            {
                modificationFee = booking.TotalNights <= 1
                    ? booking.TotalAmount
                    : booking.Items.FirstOrDefault()?.PricePerNight ?? 0m;
            }

            var newNights = request.CheckoutDate.DayNumber - request.CheckinDate.DayNumber;

            // Find a room of the same type for the new dates before touching anything, so a
            // failed modification never leaves the booking half-changed.
            var reassignments = new List<(BookingItem Item, long NewRoomId, decimal PricePerNight)>();
            foreach (var item in booking.Items)
            {
                var available = await GetAvailableRoomsExcludingBookingAsync(item.RoomTypeId, request.CheckinDate, request.CheckoutDate, bookingId);
                if (available.Count == 0)
                    throw new RoomNotAvailableException();

                var pricePerNight = await GetEffectivePriceAsync(available[0].Id, request.CheckinDate, item.RoomType.BasePrice);
                reassignments.Add((item, available[0].Id, pricePerNight));
            }

            // Free the old date range for each item's current room.
            foreach (var item in booking.Items.Where(i => i.RoomId.HasValue))
            {
                var oldAvailability = await _context.RoomAvailabilities
                    .Where(a => a.RoomId == item.RoomId && a.Date >= booking.CheckinDate && a.Date < booking.CheckoutDate)
                    .ToListAsync();
                oldAvailability.ForEach(a => a.Status = RoomAvailabilityStatus.Free);
            }

            var breakfastFee = booking.IncludeBreakfast
                ? Math.Round(booking.Hotel.BreakfastPrice * newNights * booking.Guests.Count, 2)
                : 0m;

            var newTotal = 0m;
            foreach (var (item, newRoomId, pricePerNight) in reassignments)
            {
                item.RoomId = newRoomId;
                item.Nights = newNights;
                item.PricePerNight = pricePerNight;
                item.TotalPrice = pricePerNight * newNights;
                newTotal += item.TotalPrice;
            }
            newTotal += breakfastFee;

            booking.CheckinDate = request.CheckinDate;
            booking.CheckoutDate = request.CheckoutDate;
            booking.TotalNights = newNights;
            booking.TotalAmount = newTotal;
            booking.PlatformFee = Math.Round(newTotal * booking.PlatformFeeRate, 2);
            booking.OwnerAmount = Math.Round(newTotal * (1 - booking.PlatformFeeRate), 2);
            booking.BreakfastFee = breakfastFee;
            booking.ModificationFee = modificationFee > 0 ? modificationFee : null;
            booking.LastModifiedAt = DateTime.UtcNow;

            var neededReapproval = false;
            if (booking.Status == BookingStatus.Confirmed)
            {
                booking.Status = booking.Hotel.AutoAcceptBookings ? BookingStatus.Confirmed : BookingStatus.Pending;
                neededReapproval = booking.Status == BookingStatus.Pending;
            }

            foreach (var (_, newRoomId, _) in reassignments)
            {
                await BlockRoomDatesAsync(newRoomId, request.CheckinDate, request.CheckoutDate);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new RoomNotAvailableException();
            }

            // CHANGED BY AI (2026-07-13): please review. New notifications for the Notifications
            // feature — the guest is always told their dates changed; the owner is only told if
            // it now needs their re-approval (auto-accept hotels don't need this).
            var feeNote = modificationFee > 0 ? $" A ${modificationFee:0.00} late-modification fee applies." : "";
            await _notificationService.CreateAsync(
                booking.UserId, NotificationType.BookingModified.ToString(),
                "Booking dates changed",
                $"Your booking at {booking.Hotel.Name} was updated to {request.CheckinDate:yyyy-MM-dd} → {request.CheckoutDate:yyyy-MM-dd}.{feeNote}",
                relatedBookingId: booking.Id);

            if (neededReapproval)
            {
                await _notificationService.CreateAsync(
                    booking.Hotel.OwnerId, NotificationType.NewBooking.ToString(),
                    "Booking needs re-approval",
                    $"A guest changed their booking dates at {booking.Hotel.Name} ({request.CheckinDate:yyyy-MM-dd} → {request.CheckoutDate:yyyy-MM-dd}) — it needs your approval again.",
                    relatedBookingId: booking.Id);
            }

            return await GetByIdAsync(userId, false, bookingId);
        }

        // Helper
        private async Task<List<Room>> GetAvailableRoomsAsync(long roomTypeId, DateOnly checkin, DateOnly checkout, int qty)
        {
            var allRooms = await _context.Rooms
                .Where(rt => rt.RoomTypeId == roomTypeId && rt.Status == RoomStatus.Available)
                .ToListAsync();

            // CHANGED BY AI (2026-07-13): please review. Two fixes here:
            // 1. Missing parentheses meant a room with ANY "Blocked" availability record, on ANY
            //    date ever, was excluded from every future availability check regardless of the
            //    requested date range. Now the date range correctly applies to both Booked and
            //    Blocked statuses.
            // 2. Checkout day is now exclusive (< instead of <=), matching BlockRoomDatesAsync
            //    (which never marks the checkout day itself as occupied). Previously, checking in
            //    on the exact day another stay/block ended was incorrectly rejected.
            var blockedRoomIds = await _context.RoomAvailabilities
                .Where(a =>
                a.Date >= checkin &&
                a.Date < checkout &&
                (a.Status == RoomAvailabilityStatus.Booked || a.Status == RoomAvailabilityStatus.Blocked)
                ).Select(a => a.RoomId)
                .Distinct()
                .ToListAsync();

            return allRooms
                 .Where(r => !blockedRoomIds.Contains(r.Id))
                 .Take(qty)
                 .ToList();
        }

        // CHANGED BY AI (2026-07-13): please review. Same as GetAvailableRoomsAsync, but for
        // ModifyDatesAsync — excludes availability rows belonging to the booking being modified,
        // since those are about to be freed as part of the same operation (otherwise a booking
        // modified to overlap its own current dates would look falsely unavailable).
        private async Task<List<Room>> GetAvailableRoomsExcludingBookingAsync(long roomTypeId, DateOnly checkin, DateOnly checkout, long excludeBookingId)
        {
            var allRooms = await _context.Rooms
                .Where(rt => rt.RoomTypeId == roomTypeId && rt.Status == RoomStatus.Available)
                .ToListAsync();

            var blockedRoomIds = await _context.RoomAvailabilities
                .Where(a =>
                    a.Date >= checkin &&
                    a.Date < checkout &&
                    (a.Status == RoomAvailabilityStatus.Booked || a.Status == RoomAvailabilityStatus.Blocked) &&
                    !_context.BookingItems.Any(bi => bi.BookingId == excludeBookingId && bi.RoomId == a.RoomId))
                .Select(a => a.RoomId)
                .Distinct()
                .ToListAsync();

            return allRooms.Where(r => !blockedRoomIds.Contains(r.Id)).ToList();
        }

        private async Task<decimal> GetEffectivePriceAsync(long roomId, DateOnly checkinDate, decimal basePrice)
        {
            var override_ = await _context.RoomAvailabilities
                .Where(a => a.RoomId == roomId &&
                a.Date == checkinDate &&
                a.PriceOverride.HasValue)
                .Select(a => a.PriceOverride)
                .FirstOrDefaultAsync();

            return override_ ?? basePrice;
        }
        private async Task BlockRoomDatesAsync(long roomId, DateOnly from, DateOnly to)
        {
            var existing = await _context.RoomAvailabilities
                .Where(a => a.RoomId == roomId && a.Date >= from && a.Date < to)
                .ToDictionaryAsync(a => a.Date);

            var current = from;
            while (current < to)
            {
                if (existing.TryGetValue(current, out var record))
                    record.Status = RoomAvailabilityStatus.Booked;
                else
                    _context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomId = roomId,
                        Date = current,
                        Status = RoomAvailabilityStatus.Booked
                    });

                current = current.AddDays(1);
            }
        }
        private static BookingDto MapToDto(Booking b) => new(
    b.Id, b.HotelId, b.Hotel.Name, b.Status.ToString(),
    b.CheckinDate, b.CheckoutDate, b.TotalNights, b.TotalAmount,
    b.SpecialRequests, b.CreatedAt,
    b.Items.Select(i => new BookingItemDto(
        i.Id, i.RoomType.Name, i.Qty, i.Nights, i.PricePerNight, i.TotalPrice
    )).ToList(),
    b.Guests.Select(g => new GuestDto(
        g.Id, g.FullName, g.PassportNo, g.Nationality, g.IsPrimary
    )).ToList(),
    b.Payment == null ? null : new PaymentSummaryDto(
        b.Payment.Id, b.Payment.Amount, b.Payment.Currency,
        b.Payment.Method, b.Payment.Status.ToString(), b.Payment.PaidAt),
    // CHANGED BY AI (2026-07-12): please review. Exposes the breakfast add-on on a booking.
    b.IncludeBreakfast, b.BreakfastFee,
    Reviewable: false, HasReview: false,
    // CHANGED BY AI (2026-07-13): please review. Real cancellation policy + modification fee.
    FreeCancellationEnabled: b.Hotel.FreeCancellationEnabled,
    FreeCancellationDaysBefore: b.Hotel.FreeCancellationDaysBefore,
    CancellationFeeType: b.Hotel.CancellationFeeType.ToString(),
    CancellationFeeValue: b.Hotel.CancellationFeeValue,
    ModificationFee: b.ModificationFee
);
    }
}
