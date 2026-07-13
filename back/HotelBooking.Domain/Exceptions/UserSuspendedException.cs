namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. New exception thrown at login when an account
    // is suspended; see AuthService.LoginAsync.
    public class UserSuspendedException : Exception
    {
        public UserSuspendedException(DateTimeOffset? until)
            : base(BuildMessage(until)) { }

        private static string BuildMessage(DateTimeOffset? until)
        {
            if (!until.HasValue || until.Value >= DateTimeOffset.MaxValue.AddDays(-1))
                return "Your account has been suspended indefinitely. Please contact support.";

            var remaining = until.Value - DateTimeOffset.UtcNow;
            var remainingText = remaining.TotalDays >= 1
                ? $"{Math.Ceiling(remaining.TotalDays)} day(s)"
                : remaining.TotalHours >= 1
                    ? $"{Math.Ceiling(remaining.TotalHours)} hour(s)"
                    : "less than an hour";

            return $"Your account has been suspended for {remainingText}, until {until.Value:yyyy-MM-dd HH:mm} UTC.";
        }
    }
}
