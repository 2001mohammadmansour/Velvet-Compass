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

            // CHANGED BY AI (2026-07-15): please review. New extra-bed columns, precision set here
            // in the first migration (see the AddBreakfastSettings -> FixBreakfastDecimalPrecision
            // follow-up in history — avoiding a repeat of that here).
            builder.Property(x => x.ExtraBedCount).HasDefaultValue(0);
            builder.Property(x => x.ExtraBedFee).HasPrecision(10, 2).HasDefaultValue(0m);

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
