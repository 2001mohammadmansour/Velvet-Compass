using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-15): please review. Join table for Hotel <-> Amenity. Restrict on the
    // Amenity side because amenities are never hard-deleted through the API (see AmenityService) —
    // this just enforces that invariant at the DB level too.
    internal class HotelAmenityConfiguration : IEntityTypeConfiguration<HotelAmenity>
    {
        public void Configure(EntityTypeBuilder<HotelAmenity> builder)
        {
            builder.HasKey(x => new { x.HotelId, x.AmenityId });

            builder.HasOne(x => x.Hotel)
                .WithMany(h => h.HotelAmenities)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Amenity)
                .WithMany(a => a.HotelAmenities)
                .HasForeignKey(x => x.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
