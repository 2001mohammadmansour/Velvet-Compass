using HotelBooking.Domain.Enum;
using Microsoft.AspNetCore.Identity;


namespace HotelBooking.Domain.Entities
{
    public class User : IdentityUser<long>
    {
        public UserRole Role { get; set; } = UserRole.Guest;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
