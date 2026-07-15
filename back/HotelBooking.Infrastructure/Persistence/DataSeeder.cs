using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Persistence;

public static class DataSeeder
{
    private const decimal PlatformFeeRate = 0.15m;
    private const decimal CancellationPenalty = 0.20m;

    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager)
    {
        if (await context.Hotels.AnyAsync())
            return;

        // ─── Users ────────────────────────────────────────────
        var owner1 = new User { UserName = "owner1@test.com", Email = "owner1@test.com", Role = UserRole.Owner };
        var owner2 = new User { UserName = "owner2@test.com", Email = "owner2@test.com", Role = UserRole.Owner };
        var guest1 = new User { UserName = "guest1@test.com", Email = "guest1@test.com", Role = UserRole.Guest };
        var guest2 = new User { UserName = "guest2@test.com", Email = "guest2@test.com", Role = UserRole.Guest };
        var admin1 = new User { UserName = "admin@test.com", Email = "admin@test.com", Role = UserRole.Admin };

        await userManager.CreateAsync(owner1, "Test1234!");
        await userManager.CreateAsync(owner2, "Test1234!");
        await userManager.CreateAsync(guest1, "Test1234!");
        await userManager.CreateAsync(guest2, "Test1234!");
        await userManager.CreateAsync(admin1, "Admin1234!");

        owner1 = await userManager.FindByEmailAsync("owner1@test.com") ?? throw new Exception("owner1 not found");
        owner2 = await userManager.FindByEmailAsync("owner2@test.com") ?? throw new Exception("owner2 not found");
        guest1 = await userManager.FindByEmailAsync("guest1@test.com") ?? throw new Exception("guest1 not found");
        guest2 = await userManager.FindByEmailAsync("guest2@test.com") ?? throw new Exception("guest2 not found");

        // ─── Hotels أول بدون RoomTypes ────────────────────────
        var hotel1 = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Grand Palace Hotel",
            Description = "فندق فاخر في قلب الرياض",
            Address = "شارع الملك فهد",
            City = "Riyadh",
            Country = "Saudi Arabia",
            StarRating = 5,
            Phone = "+966501234567",
            Email = "info@grandpalace.com",
            HotelImages = new List<HotelImage>
            {
                new() { Url = "https://placehold.co/800x600", IsPrimary = true, SortOrder = 1 }
            }
        };

        var hotel2 = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Blue Sea Resort",
            Description = "منتجع ساحلي مع إطلالة على البحر",
            Address = "الكورنيش الشمالي",
            City = "Jeddah",
            Country = "Saudi Arabia",
            StarRating = 4,
            Phone = "+966502345678",
            Email = "info@bluesea.com",
            HotelImages = new List<HotelImage>
            {
                new() { Url = "https://placehold.co/800x600", IsPrimary = true, SortOrder = 1 }
            }
        };

        var hotel3 = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Desert Rose Inn",
            Description = "فندق بوتيك بتصميم تراثي",
            Address = "حي العليا",
            City = "Riyadh",
            Country = "Saudi Arabia",
            StarRating = 3,
            Phone = "+966503456789",
            Email = "info@desertrose.com",
            HotelImages = new List<HotelImage>
            {
                new() { Url = "https://placehold.co/800x600", IsPrimary = true, SortOrder = 1 }
            }
        };

        context.Hotels.AddRange(hotel1, hotel2, hotel3);
        await context.SaveChangesAsync();

        // ─── RoomTypes لكل فندق منفصل (One-to-Many) ──────────
        // Hotel 1
        var h1Standard = new RoomType { HotelId = hotel1.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 100 };
        var h1Deluxe = new RoomType { HotelId = hotel1.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 180 };
        var h1Suite = new RoomType { HotelId = hotel1.Id, Name = "Suite", Description = "جناح فاخر", Capacity = 4, Beds = 2, BasePrice = 350 };

        // Hotel 2
        var h2Standard = new RoomType { HotelId = hotel2.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 90 };
        var h2Family = new RoomType { HotelId = hotel2.Id, Name = "Family Room", Description = "غرفة عائلية", Capacity = 5, Beds = 3, BasePrice = 200 };

        // Hotel 3
        var h3Standard = new RoomType { HotelId = hotel3.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 70 };
        var h3Deluxe = new RoomType { HotelId = hotel3.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 130 };

        context.RoomTypes.AddRange(h1Standard, h1Deluxe, h1Suite, h2Standard, h2Family, h3Standard, h3Deluxe);
        await context.SaveChangesAsync();

        // ─── Rooms ────────────────────────────────────────────
        var rooms = new List<Room>
        {
            // Hotel 1 - Standard
            new() { RoomTypeId = h1Standard.Id, RoomNumber = "101", Floor = 1, Status = RoomStatus.Available },
            new() { RoomTypeId = h1Standard.Id, RoomNumber = "102", Floor = 1, Status = RoomStatus.Available },
            new() { RoomTypeId = h1Standard.Id, RoomNumber = "103", Floor = 1, Status = RoomStatus.Maintenance },
            // Hotel 1 - Deluxe
            new() { RoomTypeId = h1Deluxe.Id, RoomNumber = "201", Floor = 2, Status = RoomStatus.Available },
            new() { RoomTypeId = h1Deluxe.Id, RoomNumber = "202", Floor = 2, Status = RoomStatus.Available },
            // Hotel 1 - Suite
            new() { RoomTypeId = h1Suite.Id, RoomNumber = "301", Floor = 3, Status = RoomStatus.Available },
            // Hotel 2 - Standard
            new() { RoomTypeId = h2Standard.Id, RoomNumber = "101", Floor = 1, Status = RoomStatus.Available },
            new() { RoomTypeId = h2Standard.Id, RoomNumber = "102", Floor = 1, Status = RoomStatus.Available },
            // Hotel 2 - Family
            new() { RoomTypeId = h2Family.Id, RoomNumber = "201", Floor = 2, Status = RoomStatus.Available },
            // Hotel 3 - Standard
            new() { RoomTypeId = h3Standard.Id, RoomNumber = "101", Floor = 1, Status = RoomStatus.Available },
            // Hotel 3 - Deluxe
            new() { RoomTypeId = h3Deluxe.Id, RoomNumber = "201", Floor = 2, Status = RoomStatus.Available },
        };

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        // مراجع سريعة
        var roomH1Std1 = rooms[0];
        var roomH1Std2 = rooms[1];
        var roomH1Dlx1 = rooms[3];
        var roomH1Suite = rooms[5];

        // ─── Bookings ─────────────────────────────────────────
        var bookingsList = new List<Booking>
        {
            // Confirmed - مستقبلي
            MakeBooking(guest1.Id, hotel1.Id, h1Standard,
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(13)),
                roomH1Std1, BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-3),
                new GuestDetail { FullName = "Ahmad Ali", Nationality = "Saudi", IsPrimary = true }
            ),

            // Confirmed - مستقبلي
            MakeBooking(guest2.Id, hotel1.Id, h1Deluxe,
                DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(24)),
                roomH1Dlx1, BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-2),
                new GuestDetail { FullName = "Sara Ahmed", Nationality = "Egyptian", IsPrimary = true }
            ),

            // Completed - الشهر الماضي
            MakeBooking(guest1.Id, hotel1.Id, h1Suite,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-17)),
                roomH1Suite, BookingStatus.Completed,
                DateTime.UtcNow.AddMonths(-1),
                new GuestDetail { FullName = "Mohammad Hassan", Nationality = "Saudi", IsPrimary = true }
            ),

            // Completed - قبل 3 أشهر
            MakeBooking(guest2.Id, hotel1.Id, h1Standard,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-95)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-92)),
                roomH1Std2, BookingStatus.Completed,
                DateTime.UtcNow.AddMonths(-3),
                new GuestDetail { FullName = "Layla Omar", Nationality = "Jordanian", IsPrimary = true }
            ),

            // Cancelled
            MakeBooking(guest1.Id, hotel1.Id, h1Deluxe,
                DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(33)),
                roomH1Dlx1, BookingStatus.Cancelled,
                DateTime.UtcNow.AddDays(-5),
                new GuestDetail { FullName = "Ahmad Ali", Nationality = "Saudi", IsPrimary = true }
            ),

            // Pending
            MakeBooking(guest2.Id, hotel2.Id, h2Standard,
                DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(18)),
                rooms[6], BookingStatus.Pending,
                DateTime.UtcNow,
                new GuestDetail { FullName = "Khaled Salem", Nationality = "Kuwaiti", IsPrimary = true }
            ),
        };
        context.Bookings.AddRange(bookingsList);
        await context.SaveChangesAsync();
        foreach (var booking in bookingsList.Where(b => b.Status != BookingStatus.Cancelled))
        {
            foreach (var item in booking.Items.Where(i => i.RoomId.HasValue))
            {
                var current = booking.CheckinDate;
                while (current < booking.CheckoutDate)
                {
                    context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomId = item.RoomId!.Value,
                        Date = current,
                        Status = RoomAvailabilityStatus.Booked
                    });
                    current = current.AddDays(1);
                }
            }
        }

        await context.SaveChangesAsync();
        // ─── Payments ─────────────────────────────────────────
        var payments = bookingsList.Select(b => new Payment
        {
            BookingId = b.Id,
            Amount = b.TotalAmount,
            Currency = "USD",
            Method = "CreditCard",
            Status = b.Status switch
            {
                BookingStatus.Confirmed => PaymentStatus.Paid,
                BookingStatus.Completed => PaymentStatus.Paid,
                BookingStatus.Cancelled => PaymentStatus.Refunded,
                _ => PaymentStatus.Initiated
            },
            TransactionRef = b.Status == BookingStatus.Pending
                ? null
                : $"TXN-{b.Id:D4}-SEED",
            PaidAt = b.Status == BookingStatus.Pending
                ? null
                : b.CreatedAt.AddHours(1)
        }).ToList();

        context.Payments.AddRange(payments);

        // ─── HotelViews ───────────────────────────────────────
        var rnd = new Random(42);
        var hotelViews = new List<HotelView>();

        foreach (var (hotel, maxViews, maxClicks) in new[]
        {
            (hotel1, 150, 40),
            (hotel2, 80,  20),
            (hotel3, 50,  15)
        })
        {
            for (int i = -30; i <= 0; i++)
            {
                hotelViews.Add(new HotelView
                {
                    HotelId = hotel.Id,
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(i)),
                    Views = rnd.Next(5, maxViews),
                    Clicks = rnd.Next(1, maxClicks)
                });
            }
        }

        context.HotelViews.AddRange(hotelViews);
        await context.SaveChangesAsync();

        context.Partners.AddRange(
            new Partner { Name = "Old City Walk", City = "Damascus", Description = "Explore historic alleys, traditional markets, and guided heritage stops through the heart of Damascus." },
            new Partner { Name = "Aleppo Citadel Route", City = "Aleppo", Description = "A guided trip through landmarks, local food corners, and the iconic citadel experience." },
            new Partner { Name = "Desert Sunset Trek", City = "Palmyra", Description = "A guided desert journey with sunset views, campsite stories, and a calm night under the stars." },
            new Partner { Name = "Coastal Sea Activities", City = "Latakia", Description = "Enjoy boat time, beach relaxation, and water activities led by local guides on the coast." },
            new Partner { Name = "Tartus Family Escape", City = "Tartus", Description = "A relaxed seaside outing for families with guided stops, snacks, and easy coastal fun." },
            new Partner { Name = "Homs Heritage Trail", City = "Homs", Description = "Discover city culture, historic sites, and local stories with a knowledgeable guide." }
        );
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Seeder completed successfully!");
    }

    // ─── Helper ───────────────────────────────────────────────
    private static Booking MakeBooking(
        long userId, long hotelId, RoomType roomType,
        DateOnly checkin, DateOnly checkout,
        Room room, BookingStatus status, DateTime createdAt,
        GuestDetail primaryGuest)
    {
        var nights = checkout.DayNumber - checkin.DayNumber;
        var total = roomType.BasePrice * nights;
        var fee = Math.Round(total * PlatformFeeRate, 2);
        var ownerAmount = Math.Round(total - fee, 2);

        decimal? penalty = null;
        decimal? refund = null;
        DateTime? cancelAt = null;

        if (status == BookingStatus.Cancelled)
        {
            penalty = Math.Round(total * CancellationPenalty, 2);
            refund = Math.Round(total - penalty.Value, 2);
            cancelAt = createdAt.AddDays(1);
        }

        return new Booking
        {
            UserId = userId,
            HotelId = hotelId,
            CheckinDate = checkin,
            CheckoutDate = checkout,
            TotalNights = nights,
            TotalAmount = total,
            PlatformFeeRate = PlatformFeeRate,
            PlatformFee = fee,
            OwnerAmount = ownerAmount,
            CancellationPenalty = penalty,
            RefundAmount = refund,
            CancelledAt = cancelAt,
            Status = status,
            CreatedAt = createdAt,
            Items = new List<BookingItem>
            {
                new()
                {
                    RoomTypeId    = roomType.Id,
                    RoomId        = room.Id,
                    Nights        = nights,
                    PricePerNight = roomType.BasePrice,
                    TotalPrice    = total,
                    Qty           = 1
                }
            },
            Guests = new List<GuestDetail> { primaryGuest }
        };
    }
}
