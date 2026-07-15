using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.BasePrice).HasPrecision(10, 2);
            builder.Property(x => x.Capacity).IsRequired();
            builder.Property(x => x.Beds).IsRequired();

            // CHANGED BY AI (2026-07-15): please review. New extra-bed system columns. Explicit
            // HasDefaultValue is required (not just the C# property initializer) so the migration
            // can add these as NOT NULL on the existing, already-populated RoomTypes table.
            builder.Property(x => x.AllowExtraBed).HasDefaultValue(false);
            builder.Property(x => x.MaxExtraBeds).HasDefaultValue(0);
            builder.Property(x => x.ExtraBedPriceType).HasConversion<string>().HasDefaultValue(HotelBooking.Domain.Enum.ExtraBedPriceType.Percentage);
            builder.Property(x => x.ExtraBedPriceForOneBed).HasPrecision(10, 2).HasDefaultValue(0m);
            builder.Property(x => x.ExtraBedPriceForTwoBeds).HasPrecision(10, 2).HasDefaultValue(0m);

            builder.HasOne(r => r.Hotel).
                WithMany(r => r.RoomTypes)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.RoomTypeImages)
                .WithOne(r => r.RoomType)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
