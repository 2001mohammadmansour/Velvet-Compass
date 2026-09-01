using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.TotalAmount).HasPrecision(10, 2);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.SpecialRequests).HasMaxLength(1000);
            builder.Property(x => x.PlatformFeeRate).HasPrecision(5, 2);
            builder.Property(x => x.PlatformFee).HasPrecision(10, 2);
            builder.Property(x => x.OwnerAmount).HasPrecision(10, 2);
            builder.Property(x => x.CancellationPenalty).HasPrecision(10, 2);
            builder.Property(x => x.RefundAmount).HasPrecision(10, 2);
            builder.Property(x => x.BreakfastFee).HasPrecision(10, 2);
            builder.Property(x => x.ModificationFee).HasPrecision(10, 2);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Hotel)
                   .WithMany()
                   .HasForeignKey(x => x.HotelId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                   .WithOne(x => x.Booking)
                   .HasForeignKey(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Guests)
                   .WithOne(x => x.Booking)
                   .HasForeignKey(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Payment)
                   .WithOne(x => x.Booking)
                   .HasForeignKey<Payment>(x => x.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
