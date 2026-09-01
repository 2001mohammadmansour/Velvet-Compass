using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class PlatformSettingsConfiguration : IEntityTypeConfiguration<PlatformSettings>
    {
        public void Configure(EntityTypeBuilder<PlatformSettings> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ShamCashWallet).HasMaxLength(100);
            builder.Property(x => x.ShamCashQrUrl).HasMaxLength(500);
        }
    }
}
