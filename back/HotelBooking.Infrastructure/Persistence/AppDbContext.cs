using HotelBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<long>, long>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<HotelImage> HotelImages => Set<HotelImage>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RoomAvailability> RoomAvailabilities => Set<RoomAvailability>();
        public DbSet<RoomTypeImage> RoomTypeImages => Set<RoomTypeImage>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<GuestDetail> GuestDetails => Set<GuestDetail>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<HotelView> HotelViews => Set<HotelView>();
        public DbSet<HotelRequest> HotelRequests => Set<HotelRequest>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Partner> Partners => Set<Partner>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<HotelAmenity> HotelAmenities => Set<HotelAmenity>();
        public DbSet<RoomTypeAmenity> RoomTypeAmenities => Set<RoomTypeAmenity>();
        public DbSet<SeasonalPriceRule> SeasonalPriceRules => Set<SeasonalPriceRule>();
        public DbSet<OccupancyPriceTier> OccupancyPriceTiers => Set<OccupancyPriceTier>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<long>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<long>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<long>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<long>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<long>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaims");
        }


    }
}
