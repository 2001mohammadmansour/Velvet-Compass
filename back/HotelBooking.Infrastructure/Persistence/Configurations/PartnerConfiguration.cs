using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
    {
        public void Configure(EntityTypeBuilder<Partner> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            // Stored as a JSON array column (EF Core primitive collection).
            builder.Property(x => x.Cities).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.ImageUrl).HasMaxLength(500);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(50).HasDefaultValue("Other");
            builder.Property(x => x.WebsiteUrl).HasMaxLength(500);
        }
    }
}
