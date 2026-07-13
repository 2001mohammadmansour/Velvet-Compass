using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
            builder.Property(r => r.Notes).HasMaxLength(500);
            builder.Property(r => r.Status).HasConversion<string>();

            builder.HasOne(r => r.RoomType)
                .WithMany(r => r.Rooms)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Availabilities)
                .WithOne(r => r.Room)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
