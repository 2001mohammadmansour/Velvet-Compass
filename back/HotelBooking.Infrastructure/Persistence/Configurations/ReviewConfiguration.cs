using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    // CHANGED BY AI (2026-07-13): please review. New config for the Reviews feature.
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.BookingId).IsUnique();

            builder.Property(x => x.OverallScore).HasPrecision(3, 1);
            builder.Property(x => x.Comment).HasMaxLength(2000).IsRequired();

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Hotel)
                .WithMany()
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RoomType)
                .WithMany()
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Guest)
                .WithMany()
                .HasForeignKey(x => x.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
