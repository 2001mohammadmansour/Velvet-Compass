using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    internal class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasKey(h => h.Id);

            builder.Property(n => n.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.Address).IsRequired().HasMaxLength(500);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Country).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Phone).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
            builder.Property(x => x.StarRating).IsRequired();
            builder.Property(x => x.BreakfastPrice).HasPrecision(10, 2);

            // CHANGED BY AI (2026-07-13): please review. New per-hotel cancellation policy
            // columns. Explicit HasDefaultValue is required here (not just the C# property
            // initializer) so the migration can add these as NOT NULL columns on the existing,
            // already-populated Hotels table.
            builder.Property(x => x.FreeCancellationEnabled).HasDefaultValue(true);
            builder.Property(x => x.FreeCancellationDaysBefore).HasDefaultValue(2);
            builder.Property(x => x.CancellationFeeType).HasConversion<string>().HasDefaultValue(HotelBooking.Domain.Enum.CancellationFeeType.Percentage);
            builder.Property(x => x.CancellationFeeValue).HasPrecision(10, 2).HasDefaultValue(20m);


            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.HotelImages)
                .WithOne(x => x.Hotel)
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(h => h.RoomTypes)
                .WithOne(h => h.Hotel)
                .HasForeignKey(h => h.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
