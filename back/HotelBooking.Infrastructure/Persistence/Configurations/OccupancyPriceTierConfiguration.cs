using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-15): please review. Owner-defined demand-based occupancy surge tiers.
    internal class OccupancyPriceTierConfiguration : IEntityTypeConfiguration<OccupancyPriceTier>
    {
        public void Configure(EntityTypeBuilder<OccupancyPriceTier> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AdjustmentType).HasConversion<string>();
            builder.Property(x => x.AdjustmentValue).HasPrecision(10, 2);

            builder.HasOne(x => x.Hotel)
                .WithMany(h => h.OccupancyPriceTiers)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
