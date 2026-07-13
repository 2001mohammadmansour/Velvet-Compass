using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
    {
        public void Configure(EntityTypeBuilder<BookingItem> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PricePerNight).HasPrecision(10, 2);
            builder.Property(x => x.TotalPrice).HasPrecision(10, 2);

            builder.HasOne(x => x.RoomType)
                   .WithMany()
                   .HasForeignKey(x => x.RoomTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Room)
                   .WithMany()
                   .HasForeignKey(x => x.RoomId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
