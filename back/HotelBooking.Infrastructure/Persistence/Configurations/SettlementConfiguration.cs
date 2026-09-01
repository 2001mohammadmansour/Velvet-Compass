using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
    {
        public void Configure(EntityTypeBuilder<Settlement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PeriodLabel).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.OwnerCredit).HasPrecision(12, 2);
            builder.Property(x => x.PlatformCommission).HasPrecision(12, 2);
            builder.Property(x => x.NetAmount).HasPrecision(12, 2);

            builder.HasOne(x => x.Hotel)
                   .WithMany()
                   .HasForeignKey(x => x.HotelId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
