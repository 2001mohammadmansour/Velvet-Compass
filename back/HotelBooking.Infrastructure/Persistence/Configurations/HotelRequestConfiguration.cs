using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class HotelRequestConfiguration : IEntityTypeConfiguration<HotelRequest>
    {
        public void Configure(EntityTypeBuilder<HotelRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.DocumentDataUrl).HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Hotel)
                .WithMany()
                .HasForeignKey(x => x.HotelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
