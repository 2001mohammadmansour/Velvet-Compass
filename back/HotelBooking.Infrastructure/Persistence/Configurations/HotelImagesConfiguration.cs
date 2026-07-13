using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class HotelImagesConfiguration : IEntityTypeConfiguration<HotelImage>
    {
        public void Configure(EntityTypeBuilder<HotelImage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Caption).HasMaxLength(255);
        }



    }
}
