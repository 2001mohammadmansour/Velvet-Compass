using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class GuestDetailConfiguration : IEntityTypeConfiguration<GuestDetail>
    {
        public void Configure(EntityTypeBuilder<GuestDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.PassportNo).HasMaxLength(50);
            builder.Property(x => x.Nationality).HasMaxLength(100);
        }
    }
}
