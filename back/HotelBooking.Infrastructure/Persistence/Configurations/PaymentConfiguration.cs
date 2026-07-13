using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Amount).HasPrecision(10, 2);
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.Currency).HasMaxLength(10);
            builder.Property(x => x.Method).HasMaxLength(50);
            builder.Property(x => x.TransactionRef).HasMaxLength(255);
        }
    }
}
