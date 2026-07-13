using System.Text;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Infrastructure.Persistence;
using HotelBooking.Infrastructure.Services;
using HotelBooking.Infrastructure.Services.Auth;
using HotelBooking.Infrastructure.Services.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace HotelBooking.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
        public static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole<long>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var secret = configuration["Jwt:Secret"];
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            return services;
        }
        public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
        {
            services.AddOpenApiDocument(options =>
            {
                options.Title = "HotelBooking API";
                options.Version = "v1";

                options.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT token"
                });

                options.OperationProcessors.Add(
                    new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer")
                );
            });

            return services;
        }
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
        public static IServiceCollection AddHotelServices(this IServiceCollection services)
        {
            services.AddScoped<IHotelService, HotelServices>();
            return services;
        }

        public static IServiceCollection AddRoomTypeServices(this IServiceCollection services)
        {
            services.AddScoped<IRoomTypeService, RoomTypeServices>();
            return services;
        }

        public static IServiceCollection AddRoomServices(this IServiceCollection services)
        {
            services.AddScoped<IRoomService, RoomServices>();
            return services;
        }

        public static IServiceCollection AddBookingServices(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();
            return services;
        }

        public static IServiceCollection AddPaymentServices(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();
            return services;
        }
        public static IServiceCollection AddOwnerDashBoardServices(this IServiceCollection services)
        {
            services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
            return services;
        }

        public static IServiceCollection AddAdminDashBoardServices(this IServiceCollection services)
        {
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            return services;
        }

        public static IServiceCollection AddHotelRequestServices(this IServiceCollection services)
        {
            services.AddScoped<IHotelRequestService, HotelRequestService>();
            return services;
        }

        // CHANGED BY AI (2026-07-13): please review. New DI registration for the Reviews feature.
        public static IServiceCollection AddReviewServices(this IServiceCollection services)
        {
            services.AddScoped<IReviewService, ReviewService>();
            return services;
        }

        // CHANGED BY AI (2026-07-13): please review. New DI registration for the Notifications
        // feature.
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }

        // CHANGED BY AI (2026-07-13): please review. New DI registration for the Trips feature.
        public static IServiceCollection AddTripServices(this IServiceCollection services)
        {
            services.AddScoped<ITripService, TripService>();
            return services;
        }

    }
}
