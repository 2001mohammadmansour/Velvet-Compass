namespace HotelBooking.Application.DTOs.Common
{
    public record PagedResult<T>
    (
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize
        )
    {
        public int totalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNext => Page < TotalCount;
        public bool HasPrevious => Page > 1;

    };
}
