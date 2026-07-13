using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    internal class RoomAvailabilityConfiguration : IEntityTypeConfiguration<RoomAvailability>
    {
        public void Configure(EntityTypeBuilder<RoomAvailability> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.PriceOverride).HasPrecision(10, 2);

            builder.HasIndex(x => new { x.RoomId, x.Date }).IsUnique();
        }
    }
}
