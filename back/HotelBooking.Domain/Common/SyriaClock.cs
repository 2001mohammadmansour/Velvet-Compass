namespace HotelBooking.Domain.Common
{
    // All hotels on this platform are in Syria (UTC+3, no DST observed). Data is stored/compared in
    // UTC, but "is this calendar date in the past" checks (check-in date, cancellation cutoffs, ...)
    // need the Syria-local date: near local midnight, DateTime.UtcNow can still show the previous
    // day for up to 3 hours, which let guests book a check-in date that had already started locally.
    public static class SyriaClock
    {
        private static readonly TimeSpan Offset = TimeSpan.FromHours(3);

        public static DateTime Now => DateTime.UtcNow + Offset;
        public static DateOnly Today => DateOnly.FromDateTime(Now);
    }
}
