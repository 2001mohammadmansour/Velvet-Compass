using HotelBooking.Application.DTOs.Reviews;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-13): please review. New service backing the Reviews feature.
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        public ReviewService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ReviewDto> SubmitAsync(long userId, long bookingId, SubmitReviewRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.Items).ThenInclude(i => i.RoomType)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId)
                ?? throw new BookingNotFoundException(bookingId);

            if (booking.Status == BookingStatus.Cancelled)
                throw new ReviewNotEligibleException("Cancelled bookings can't be reviewed.");

            // CHANGED BY AI (2026-07-13): please review. BookingStatus.Completed is never actually
            // assigned anywhere in this codebase outside of seed data (no background job exists to
            // transition a booking after checkout), so eligibility is based on the checkout date
            // having passed instead of a status this app never sets in real usage.
            if (booking.CheckoutDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ReviewNotEligibleException("You can review this stay after your checkout date has passed.");

            if (await _context.Reviews.AnyAsync(r => r.BookingId == bookingId))
                throw new AlreadyReviewedException(bookingId);

            var primaryItem = booking.Items.FirstOrDefault()
                ?? throw new ReviewNotEligibleException("This booking has no room to review.");

            var review = new Review
            {
                BookingId = bookingId,
                HotelId = booking.HotelId,
                RoomTypeId = primaryItem.RoomTypeId,
                GuestId = userId,
                Staff = request.Staff,
                Location = request.Location,
                Facilities = request.Facilities,
                Cleanliness = request.Cleanliness,
                Comfort = request.Comfort,
                Value = request.Value,
                OverallScore = ComputeOverallScore(request),
                Comment = request.Comment,
            };

            _context.Reviews.Add(review);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Race: two submissions for the same booking landed at once; the unique index on
                // BookingId rejects the second at the database level.
                throw new AlreadyReviewedException(bookingId);
            }

            var guestName = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserName).FirstOrDefaultAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature.
            await _notificationService.CreateAsync(
                booking.Hotel.OwnerId, NotificationType.NewReview.ToString(),
                "New review received",
                $"{guestName ?? "A guest"} left a {review.OverallScore}/10 review for {booking.Hotel.Name}.",
                relatedBookingId: booking.Id);

            return new ReviewDto(review.Id, primaryItem.RoomType.Name, guestName ?? "Guest", review.OverallScore, review.Comment, review.CreatedAt);
        }

        // Weighted average matching the frontend's own preview calculation (MyBookings.js) —
        // cleanliness and comfort count double. Computed here, server-side, so a client can never
        // submit an arbitrary overall score.
        private static decimal ComputeOverallScore(SubmitReviewRequest r)
        {
            var sum = r.Staff + r.Location + r.Facilities + (r.Cleanliness * 2) + (r.Comfort * 2) + r.Value;
            return Math.Round(sum / 8m, 1);
        }

        // CHANGED BY AI (2026-07-13): please review. New admin moderation operations.
        public async Task<ReviewDetailDto> GetByIdAsync(long reviewId)
        {
            var r = await _context.Reviews
                .Include(x => x.Hotel)
                .Include(x => x.RoomType)
                .Include(x => x.Guest)
                .FirstOrDefaultAsync(x => x.Id == reviewId)
                ?? throw new ReviewNotFoundException(reviewId);

            return new ReviewDetailDto(
                r.Id, r.BookingId, r.Hotel.Name, r.RoomType.Name, r.Guest.UserName ?? "Guest",
                r.Staff, r.Location, r.Facilities, r.Cleanliness, r.Comfort, r.Value,
                r.OverallScore, r.Comment, r.CreatedAt
            );
        }

        public async Task DeleteAsync(long reviewId)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new ReviewNotFoundException(reviewId);

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public Task<ReviewSummaryDto> GetHotelReviewsAsync(long hotelId)
            => BuildSummaryAsync(_context.Reviews.Where(r => r.HotelId == hotelId));

        public Task<ReviewSummaryDto> GetRoomTypeReviewsAsync(long hotelId, long roomTypeId)
            => BuildSummaryAsync(_context.Reviews.Where(r => r.HotelId == hotelId && r.RoomTypeId == roomTypeId));

        private static async Task<ReviewSummaryDto> BuildSummaryAsync(IQueryable<Review> query)
        {
            var reviews = await query
                .Include(r => r.Guest)
                .Include(r => r.RoomType)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (reviews.Count == 0)
                return new ReviewSummaryDto(0, 0, null, new List<ReviewDto>());

            var categoryAverages = new CategoryAveragesDto(
                Math.Round((decimal)reviews.Average(r => r.Staff), 1),
                Math.Round((decimal)reviews.Average(r => r.Location), 1),
                Math.Round((decimal)reviews.Average(r => r.Facilities), 1),
                Math.Round((decimal)reviews.Average(r => r.Cleanliness), 1),
                Math.Round((decimal)reviews.Average(r => r.Comfort), 1),
                Math.Round((decimal)reviews.Average(r => r.Value), 1)
            );

            return new ReviewSummaryDto(
                Math.Round(reviews.Average(r => r.OverallScore), 1),
                reviews.Count,
                categoryAverages,
                reviews.Select(r => new ReviewDto(r.Id, r.RoomType.Name, r.Guest.UserName ?? "Guest", r.OverallScore, r.Comment, r.CreatedAt)).ToList()
            );
        }
    }
}
