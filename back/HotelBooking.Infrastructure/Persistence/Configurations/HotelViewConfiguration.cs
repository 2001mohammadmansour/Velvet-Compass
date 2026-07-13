using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations;

public class HotelViewConfiguration : IEntityTypeConfiguration<HotelView>
{
    public void Configure(EntityTypeBuilder<HotelView> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // فندق + يوم ما يتكرر
        builder.HasIndex(x => new { x.HotelId, x.Date }).IsUnique();

        builder.HasOne(x => x.Hotel)
               .WithMany()
               .HasForeignKey(x => x.HotelId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
