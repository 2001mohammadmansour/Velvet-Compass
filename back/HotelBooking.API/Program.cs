using HotelBooking.API.Extensions;
using HotelBooking.API.Middleware;
using HotelBooking.API.Services;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityService();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithAuth();
builder.Services.AddApplicationServices();
builder.Services.AddHotelServices();
builder.Services.AddRoomTypeServices();
builder.Services.AddRoomServices();
builder.Services.AddBookingServices();
builder.Services.AddPaymentServices();
builder.Services.AddOwnerDashBoardServices();
builder.Services.AddAdminDashBoardServices();
builder.Services.AddHotelRequestServices();
builder.Services.AddReviewServices();
builder.Services.AddNotificationServices();
builder.Services.AddPartnerServices();
builder.Services.AddAmenityServices();
builder.Services.AddRoomPricingServices();
// CHANGED BY AI (2026-07-13): please review. Backs the new hotel/room-type photo upload
// endpoints — saves files under wwwroot/uploads and returns a public URL.
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(","))
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi(options =>
{
    options.DocumentTitle = "HotelBooking API";
    options.Path = "";
});

app.UseMiddleware<ExceptionMiddleware>();
// CHANGED BY AI (2026-07-13): please review. Serves uploaded hotel/room-type photos from
// wwwroot/uploads (created on first upload if it doesn't exist yet).
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    try
    {
        await DataSeeder.SeedAsync(context, userManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeder Error: {ex.Message}");
        Console.WriteLine(ex.InnerException?.Message);
        throw;
    }
}

app.Run();
