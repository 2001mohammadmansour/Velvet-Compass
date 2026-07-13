using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-13): please review. New config for the Trips feature.
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Price).HasPrecision(10, 2);
            builder.Property(x => x.PriceLabel).HasMaxLength(50);
            builder.Property(x => x.Type).HasMaxLength(100);
            builder.Property(x => x.Duration).HasMaxLength(50);
            builder.Property(x => x.Difficulty).HasMaxLength(50);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        }
    }
}
