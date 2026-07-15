using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-15): please review. Owner-defined seasonal/holiday pricing rules.
    internal class SeasonalPriceRuleConfiguration : IEntityTypeConfiguration<SeasonalPriceRule>
    {
        public void Configure(EntityTypeBuilder<SeasonalPriceRule> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AdjustmentType).HasConversion<string>();
            builder.Property(x => x.AdjustmentValue).HasPrecision(10, 2);

            builder.HasOne(x => x.Hotel)
                .WithMany(h => h.SeasonalPriceRules)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
