using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class TwoFactorChallengeConfiguration : IEntityTypeConfiguration<TwoFactorChallenge>
    {
        public void Configure(EntityTypeBuilder<TwoFactorChallenge> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ChallengeToken).IsRequired().HasMaxLength(500);
            builder.HasIndex(x => x.ChallengeToken).IsUnique();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
