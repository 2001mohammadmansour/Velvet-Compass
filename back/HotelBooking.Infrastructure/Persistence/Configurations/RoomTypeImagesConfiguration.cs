using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class RoomTypeImagesConfiguration : IEntityTypeConfiguration<RoomTypeImage>
    {
        public void Configure(EntityTypeBuilder<RoomTypeImage> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Url).IsRequired().HasMaxLength(1000);
            builder.Property(r => r.Caption).HasMaxLength(255);
        }
    }
}
