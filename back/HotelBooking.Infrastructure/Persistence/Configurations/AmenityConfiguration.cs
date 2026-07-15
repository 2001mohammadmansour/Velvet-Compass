using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-15): please review. Admin-managed amenity catalog.
    internal class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Icon).HasMaxLength(50);
            builder.Property(x => x.Scope).HasConversion<string>();
            builder.Property(x => x.IsActive).HasDefaultValue(true);
        }
    }
}
