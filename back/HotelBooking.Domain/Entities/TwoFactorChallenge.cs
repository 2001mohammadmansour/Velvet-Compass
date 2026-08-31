namespace HotelBooking.Domain.Entities
{
    public class TwoFactorChallenge : BaseEntity
    {
        public long UserId { get; set; }
        public string ChallengeToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public int FailedAttempts { get; set; } = 0;

        // Navigation
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired && FailedAttempts < 5;

    }
}
