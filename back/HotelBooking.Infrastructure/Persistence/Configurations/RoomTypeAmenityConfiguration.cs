using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-15): please review. Join table for RoomType <-> Amenity. Same
    // Restrict-on-Amenity-side reasoning as HotelAmenityConfiguration.
    internal class RoomTypeAmenityConfiguration : IEntityTypeConfiguration<RoomTypeAmenity>
    {
        public void Configure(EntityTypeBuilder<RoomTypeAmenity> builder)
        {
            builder.HasKey(x => new { x.RoomTypeId, x.AmenityId });

            builder.HasOne(x => x.RoomType)
                .WithMany(rt => rt.RoomTypeAmenities)
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Amenity)
                .WithMany(a => a.RoomTypeAmenities)
                .HasForeignKey(x => x.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
