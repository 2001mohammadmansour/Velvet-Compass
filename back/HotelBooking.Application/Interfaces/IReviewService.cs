using HotelBooking.Application.DTOs.Reviews;

namespace HotelBooking.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> SubmitAsync(long userId, long bookingId, SubmitReviewRequest request);
        Task<ReviewSummaryDto> GetHotelReviewsAsync(long hotelId);
        Task<ReviewSummaryDto> GetRoomTypeReviewsAsync(long hotelId, long roomTypeId);
        // CHANGED BY AI (2026-07-13): please review. New admin moderation operations — view full
        // review detail and delete a review (e.g. if it's toxic/abusive).
        Task<ReviewDetailDto> GetByIdAsync(long reviewId);
        Task DeleteAsync(long reviewId);
    }
}
