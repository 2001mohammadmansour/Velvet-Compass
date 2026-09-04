using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.Infrastructure.Persistence;

public static class DataSeeder
{
    private const decimal PlatformFeeRate = 0.15m;
    private const decimal CancellationPenalty = 0.20m;

    // Generic room-photo pool shared across room types that don't have dedicated photos of their
    // own (see wwwroot/seed-images/room-types). Shuffled once with a fixed seed so the assignment
    // is reproducible but still looks "random" across room types.
    private static readonly string[] RoomImagePool =
    {
        "pool01.jpg", "pool02.jpg", "pool03.jpg", "pool04.jpg", "pool05.jpg",
        "pool06.jpg", "pool07.jpg", "pool08.jpg", "pool09.jpg", "pool10.jpg",
        "pool11.jpg", "pool12.jpg", "pool13.jpg", "pool14.jpg", "pool15.jpg"
    };

    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        if (await context.Hotels.AnyAsync())
            return;

        var baseUrl = configuration["Uploads:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5193";
        var rnd = new Random(42);
        var shuffledRoomPool = RoomImagePool.OrderBy(_ => rnd.Next()).ToArray();
        var poolIndex = 0;

        // ─── Image helpers (files live under wwwroot/seed-images) ─────
        string HotelImgUrl(string city, string file) => $"{baseUrl}/seed-images/hotels/{city}/{file}";
        string RoomImgUrl(string file) => $"{baseUrl}/seed-images/room-types/{file}";

        List<HotelImage> HotelPhotos(string city, params string[] files) =>
            files.Select((f, i) => new HotelImage { Url = HotelImgUrl(city, f), IsPrimary = i == 0, SortOrder = i + 1 }).ToList();

        List<RoomTypeImage> DedicatedRoomPhotos(params string[] files) =>
            files.Select((f, i) => new RoomTypeImage { Url = RoomImgUrl(f), IsPrimary = i == 0, SortOrder = i + 1 }).ToList();

        List<RoomTypeImage> PooledRoomPhotos(int count = 2)
        {
            var list = new List<RoomTypeImage>();
            for (int i = 0; i < count; i++)
            {
                var file = shuffledRoomPool[poolIndex % shuffledRoomPool.Length];
                poolIndex++;
                list.Add(new RoomTypeImage { Url = RoomImgUrl(file), IsPrimary = i == 0, SortOrder = i + 1 });
            }
            return list;
        }

        List<Room> MakeRooms(RoomType roomType, int floor, params string[] roomNumbers) =>
            roomNumbers.Select(num => new Room
            {
                RoomTypeId = roomType.Id,
                RoomNumber = num,
                Floor = floor,
                Status = RoomStatus.Available
            }).ToList();

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

        // ─── Hotels (10 فنادق حقيقية موزعة على المحافظات السورية) ─
        var chamPalace = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Cham Palace Hotel",
            Description = "فندق فاخر من فئة خمس نجوم يقع في منطقة الميسلون في دمشق، ويضم غرفاً وأجنحة ومطاعم ومسبحاً ومركزاً للياقة البدنية ومركزاً للأعمال.",
            Address = "3 Maysaloon Street, Damascus, Syria",
            City = "Damascus",
            Country = "Syria",
            StarRating = 5,
            Phone = "+963112232300",
            Email = "chamresa@net.sy",
            HotelImages = HotelPhotos("Damascus", "cham1.jpg", "cham2.jpg", "cham3.jpg")
        };

        var damaRose = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Dama Rose Hotel",
            Description = "فندق خمس نجوم يقع في منطقة أبو رمانة على شارع شكري القوتلي، ويضم غرفاً وأجنحة ومرافق تشمل مطاعم ومقهى ومسبحاً وسبا ومركزاً رياضياً ومركز أعمال.",
            Address = "Shukry Al-Qouwatly Street, Abou Roumaneh, Damascus, Syria",
            City = "Damascus",
            Country = "Syria",
            StarRating = 5,
            Phone = "+963112229200",
            Email = "info@damarose.com",
            HotelImages = HotelPhotos("Damascus", "Damarose1.jpg", "Damarose2.jpg", "Damarose3.jpg")
        };

        var fourSeasons = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Four Seasons Hotel Damascus",
            Description = "فندق فاخر يقع في وسط دمشق على شارع شكري القوتلي، بالقرب من المناطق الرئيسية والمعالم التاريخية.",
            Address = "Shukri Al Quwatli Street, Damascus, Syria",
            City = "Damascus",
            Country = "Syria",
            StarRating = 5,
            Phone = "+963113391000",
            Email = "reservations.dam@fourseasons.com",
            HotelImages = HotelPhotos("Damascus", "fourseason1.jpg", "fourseason2.jpg", "fourseason3.jpg")
        };

        var safirHoms = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Safir Homs Hotel",
            Description = "فندق يقع في منطقة الإنشاءات في مدينة حمص، ضمن منطقة سكنية حديثة وقريب من مركز الأعمال والتسوق.",
            Address = "Ragheb Al Jamali Street, Al Inshaat, Homs, Syria",
            City = "Homs",
            Country = "Syria",
            StarRating = 5,
            Phone = "+963312112400",
            Email = "reservations.homs@safirhotels.com",
            HotelImages = HotelPhotos("Homs", "safirHoms.jpg", "safirHoms2.jpg")
        };

        var seaView = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Sea View Hotel",
            Description = "فندق يقع على الكورنيش الغربي في مدينة اللاذقية، ويوفر إقامة مع إطلالة بحرية ومرافق فندقية ومطعم.",
            Address = "Western Corniche, Latakia, Syria",
            City = "Latakia",
            Country = "Syria",
            StarRating = 3,
            Phone = "+963985313000",
            Email = "info@seaview-sy.com",
            HotelImages = HotelPhotos("Lattakia", "SeaView.jpg", "SeaView2.jpg", "SeaView3.jpg")
        };

        var grandTartous = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Grand Hotel Tartous",
            Description = "فندق في طرطوس يقع على الكورنيش الشرقي، ويضم مرافق فندقية ومنطقة للأطفال ومسبحاً للأطفال وخدمات للضيوف.",
            Address = "East Corniche, Tartus, Syria",
            City = "Tartous",
            Country = "Syria",
            StarRating = 4,
            Phone = "+963984535353",
            Email = "info@grandhoteltartous.com",
            HotelImages = HotelPhotos("Tartous", "grand.jpg", "grand1.jpg", "grand2.jpg")
        };

        var orientHouse = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Orient House Hotel",
            Description = "فندق تاريخي في مدينة حماة يقع ضمن منزل قديم، ويتميز بالطابع التراثي والتصميم الداخلي التقليدي، مع مطعم وخدمات إقامة.",
            Address = "Fern Al-Ras Alley, Hama, Syria",
            City = "Hama",
            Country = "Syria",
            StarRating = 3,
            Phone = "+963332225599",
            Email = "info@orienthouse-hama.com",
            HotelImages = HotelPhotos("Hama", "OrientHouse.jpg")
        };

        var arman = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Arman Hotel",
            Description = "فندق في مدينة حلب يقع ضمن منطقة مشروع الـ3000 شقة.",
            Address = "3000 Apartments Project, Aleppo, Syria",
            City = "Aleppo",
            Country = "Syria",
            StarRating = 3,
            Phone = "+963215111555",
            Email = "info@armanhotel-aleppo.com",
            HotelImages = HotelPhotos("Aleppo", "armanAleppo1.webp", "armanAleppo2.webp")
        };

        var sheratonAleppo = new Hotel
        {
            OwnerId = owner1.Id,
            Name = "Sheraton Aleppo Hotel",
            Description = "فندق خمس نجوم في قلب مدينة حلب، قريب من باب الفرج، ويضم مطاعم وقاعات اجتماعات ومؤتمرات ومسبحاً أولمبياً ونادياً صحياً.",
            Address = "Jadet Al-Khandaq, Bab Al-Faraj, Aleppo, Syria",
            City = "Aleppo",
            Country = "Syria",
            StarRating = 5,
            Phone = "+963212121111",
            Email = "reservation@sheraton-aleppo.com",
            HotelImages = HotelPhotos("Aleppo", "SheratonAleppo1.JPG", "SheratonAleppo2.webp")
        };

        var baron = new Hotel
        {
            OwnerId = owner2.Id,
            Name = "Baron Hotel",
            Description = "فندق تاريخي معروف في مدينة حلب، يحافظ على طابعه العريق منذ أوائل القرن الماضي.",
            Address = "Baron Street, Aleppo, Syria",
            City = "Aleppo",
            Country = "Syria",
            StarRating = 3,
            Phone = "+963212110880",
            Email = "hotelbaron@scs-net.org",
            HotelImages = HotelPhotos("Aleppo", "BaronAleppo1.jpg", "BaronAleppo2.jpg")
        };

        context.Hotels.AddRange(chamPalace, damaRose, fourSeasons, safirHoms, seaView, grandTartous, orientHouse, arman, sheratonAleppo, baron);
        await context.SaveChangesAsync();

        // ─── RoomTypes لكل فندق ─────────────────────────────────
        // Cham Palace
        var chamSuperior = new RoomType { HotelId = chamPalace.Id, Name = "Superior Room", Description = "غرفة فاخرة بتصميم كلاسيكي", Capacity = 2, Beds = 1, BasePrice = 140, RoomTypeImages = PooledRoomPhotos() };
        var chamDeluxe = new RoomType { HotelId = chamPalace.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس واسعة", Capacity = 2, Beds = 1, BasePrice = 190, RoomTypeImages = PooledRoomPhotos() };
        var chamSuite = new RoomType { HotelId = chamPalace.Id, Name = "Suite", Description = "جناح فاخر بإطلالة بانورامية", Capacity = 4, Beds = 2, BasePrice = 320, RoomTypeImages = PooledRoomPhotos() };

        // Dama Rose
        var damaSuperior = new RoomType { HotelId = damaRose.Id, Name = "Superior Room", Description = "غرفة فاخرة بتصميم عصري", Capacity = 2, Beds = 1, BasePrice = 130, RoomTypeImages = PooledRoomPhotos() };
        var damaDeluxe = new RoomType { HotelId = damaRose.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 170, RoomTypeImages = PooledRoomPhotos() };
        var damaJuniorSuite = new RoomType { HotelId = damaRose.Id, Name = "Junior Suite", Description = "جناح صغير مع صالة جلوس", Capacity = 3, Beds = 2, BasePrice = 250, RoomTypeImages = PooledRoomPhotos() };
        var damaDeluxeSuite = new RoomType { HotelId = damaRose.Id, Name = "Deluxe Suite", Description = "جناح ديلوكس فسيح", Capacity = 4, Beds = 2, BasePrice = 320, RoomTypeImages = PooledRoomPhotos() };
        var damaExecutiveSuite = new RoomType { HotelId = damaRose.Id, Name = "Executive Suite", Description = "جناح تنفيذي مع خدمات إضافية", Capacity = 4, Beds = 2, BasePrice = 420, RoomTypeImages = PooledRoomPhotos() };
        var damaPresidentialSuite = new RoomType { HotelId = damaRose.Id, Name = "Presidential Suite", Description = "الجناح الرئاسي الأكبر في الفندق", Capacity = 6, Beds = 3, BasePrice = 650, RoomTypeImages = PooledRoomPhotos() };

        // Four Seasons Damascus
        var fsPremier = new RoomType { HotelId = fourSeasons.Id, Name = "Premier Room", Description = "غرفة فاخرة بإطلالة على المدينة", Capacity = 2, Beds = 1, BasePrice = 160, RoomTypeImages = PooledRoomPhotos() };
        var fsExecutiveSuite = new RoomType { HotelId = fourSeasons.Id, Name = "Executive Suite", Description = "جناح تنفيذي مع صالة منفصلة", Capacity = 3, Beds = 2, BasePrice = 350, RoomTypeImages = PooledRoomPhotos() };
        var fsRoyalSuite = new RoomType { HotelId = fourSeasons.Id, Name = "Royal Suite", Description = "الجناح الملكي بأعلى مستوى فخامة", Capacity = 4, Beds = 2, BasePrice = 700, RoomTypeImages = PooledRoomPhotos() };
        var fsResidenceOne = new RoomType { HotelId = fourSeasons.Id, Name = "Hotel Residence One Room", Description = "شقة فندقية بغرفة نوم واحدة", Capacity = 2, Beds = 1, BasePrice = 220, RoomTypeImages = PooledRoomPhotos() };
        var fsDiplomaticSuite = new RoomType { HotelId = fourSeasons.Id, Name = "Deluxe Diplomatic One-Bedroom Suite", Description = "جناح دبلوماسي بغرفة نوم منفصلة", Capacity = 3, Beds = 2, BasePrice = 480, RoomTypeImages = PooledRoomPhotos() };

        // Safir Homs
        var safirStandard = new RoomType { HotelId = safirHoms.Id, Name = "Standard Room", Description = "غرفة قياسية مريحة", Capacity = 2, Beds = 1, BasePrice = 80, RoomTypeImages = PooledRoomPhotos() };
        var safirDeluxe = new RoomType { HotelId = safirHoms.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 120, RoomTypeImages = PooledRoomPhotos() };
        var safirSuite = new RoomType { HotelId = safirHoms.Id, Name = "Suite", Description = "جناح واسع مع صالة جلوس", Capacity = 4, Beds = 2, BasePrice = 200, RoomTypeImages = PooledRoomPhotos() };

        // Sea View Latakia
        var seaStandard = new RoomType { HotelId = seaView.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 50, RoomTypeImages = PooledRoomPhotos() };
        var seaViewRoom = new RoomType { HotelId = seaView.Id, Name = "Sea View Room", Description = "غرفة بإطلالة مباشرة على البحر", Capacity = 2, Beds = 1, BasePrice = 75, RoomTypeImages = PooledRoomPhotos() };
        var seaFamily = new RoomType { HotelId = seaView.Id, Name = "Family Room", Description = "غرفة عائلية واسعة", Capacity = 4, Beds = 2, BasePrice = 110, RoomTypeImages = PooledRoomPhotos() };

        // Grand Hotel Tartous
        var grandStandard = new RoomType { HotelId = grandTartous.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 65, RoomTypeImages = PooledRoomPhotos() };
        var grandDeluxe = new RoomType { HotelId = grandTartous.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس بإطلالة على الكورنيش", Capacity = 2, Beds = 1, BasePrice = 95, RoomTypeImages = PooledRoomPhotos() };
        var grandFamily = new RoomType { HotelId = grandTartous.Id, Name = "Family Room", Description = "غرفة عائلية قريبة من مسبح الأطفال", Capacity = 5, Beds = 3, BasePrice = 150, RoomTypeImages = PooledRoomPhotos() };

        // Orient House Hama
        var orientHeritage = new RoomType { HotelId = orientHouse.Id, Name = "Heritage Room", Description = "غرفة بطابع تراثي داخل المنزل القديم", Capacity = 2, Beds = 1, BasePrice = 55, RoomTypeImages = PooledRoomPhotos() };
        var orientDeluxeHeritage = new RoomType { HotelId = orientHouse.Id, Name = "Deluxe Heritage Suite", Description = "جناح تراثي بمساحة أكبر", Capacity = 3, Beds = 2, BasePrice = 95, RoomTypeImages = PooledRoomPhotos() };

        // Arman Aleppo (له صور مخصّصة بدل المجموعة العامة)
        var armanStandard = new RoomType { HotelId = arman.Id, Name = "Standard Room", Description = "غرفة قياسية", Capacity = 2, Beds = 1, BasePrice = 45, RoomTypeImages = DedicatedRoomPhotos("armanRoom1.webp") };
        var armanDeluxe = new RoomType { HotelId = arman.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 70, RoomTypeImages = DedicatedRoomPhotos("armanRoom2.webp") };
        var armanSuite = new RoomType { HotelId = arman.Id, Name = "Suite", Description = "جناح واسع", Capacity = 4, Beds = 2, BasePrice = 120, RoomTypeImages = DedicatedRoomPhotos("armanRoom3.webp") };

        // Sheraton Aleppo
        var sheratonClassic = new RoomType { HotelId = sheratonAleppo.Id, Name = "Classic Room", Description = "غرفة كلاسيكية مريحة", Capacity = 2, Beds = 1, BasePrice = 130, RoomTypeImages = PooledRoomPhotos() };
        var sheratonDeluxe = new RoomType { HotelId = sheratonAleppo.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 180, RoomTypeImages = PooledRoomPhotos() };
        var sheratonSuite = new RoomType { HotelId = sheratonAleppo.Id, Name = "Suite", Description = "جناح فاخر", Capacity = 4, Beds = 2, BasePrice = 300, RoomTypeImages = PooledRoomPhotos() };

        // Baron Aleppo
        var baronStandard = new RoomType { HotelId = baron.Id, Name = "Standard Room", Description = "غرفة قياسية بطابع تاريخي", Capacity = 2, Beds = 1, BasePrice = 40, RoomTypeImages = PooledRoomPhotos() };
        var baronDeluxe = new RoomType { HotelId = baron.Id, Name = "Deluxe Room", Description = "غرفة ديلوكس", Capacity = 2, Beds = 1, BasePrice = 60, RoomTypeImages = PooledRoomPhotos() };

        context.RoomTypes.AddRange(
            chamSuperior, chamDeluxe, chamSuite,
            damaSuperior, damaDeluxe, damaJuniorSuite, damaDeluxeSuite, damaExecutiveSuite, damaPresidentialSuite,
            fsPremier, fsExecutiveSuite, fsRoyalSuite, fsResidenceOne, fsDiplomaticSuite,
            safirStandard, safirDeluxe, safirSuite,
            seaStandard, seaViewRoom, seaFamily,
            grandStandard, grandDeluxe, grandFamily,
            orientHeritage, orientDeluxeHeritage,
            armanStandard, armanDeluxe, armanSuite,
            sheratonClassic, sheratonDeluxe, sheratonSuite,
            baronStandard, baronDeluxe
        );
        await context.SaveChangesAsync();

        // ─── Rooms ────────────────────────────────────────────
        var chamSuperiorRooms = MakeRooms(chamSuperior, 1, "101", "102", "103");
        var chamDeluxeRooms = MakeRooms(chamDeluxe, 2, "201", "202");
        var chamSuiteRooms = MakeRooms(chamSuite, 3, "301");

        var damaSuperiorRooms = MakeRooms(damaSuperior, 1, "101", "102", "103");
        var damaDeluxeRooms = MakeRooms(damaDeluxe, 2, "201", "202");
        var damaJuniorSuiteRooms = MakeRooms(damaJuniorSuite, 3, "301", "302");
        var damaDeluxeSuiteRooms = MakeRooms(damaDeluxeSuite, 4, "401");
        var damaExecutiveSuiteRooms = MakeRooms(damaExecutiveSuite, 4, "402");
        var damaPresidentialSuiteRooms = MakeRooms(damaPresidentialSuite, 5, "501");

        var fsPremierRooms = MakeRooms(fsPremier, 1, "101", "102", "103");
        var fsExecutiveSuiteRooms = MakeRooms(fsExecutiveSuite, 2, "201", "202");
        var fsRoyalSuiteRooms = MakeRooms(fsRoyalSuite, 3, "301");
        var fsResidenceOneRooms = MakeRooms(fsResidenceOne, 3, "302", "303");
        var fsDiplomaticSuiteRooms = MakeRooms(fsDiplomaticSuite, 4, "401");

        var safirStandardRooms = MakeRooms(safirStandard, 1, "101", "102", "103");
        var safirDeluxeRooms = MakeRooms(safirDeluxe, 2, "201", "202");
        var safirSuiteRooms = MakeRooms(safirSuite, 3, "301");

        var seaStandardRooms = MakeRooms(seaStandard, 1, "101", "102");
        var seaViewRoomRooms = MakeRooms(seaViewRoom, 2, "201", "202");
        var seaFamilyRooms = MakeRooms(seaFamily, 2, "203");

        var grandStandardRooms = MakeRooms(grandStandard, 1, "101", "102");
        var grandDeluxeRooms = MakeRooms(grandDeluxe, 2, "201", "202");
        var grandFamilyRooms = MakeRooms(grandFamily, 2, "203");

        var orientHeritageRooms = MakeRooms(orientHeritage, 1, "101", "102");
        var orientDeluxeHeritageRooms = MakeRooms(orientDeluxeHeritage, 1, "103");

        var armanStandardRooms = MakeRooms(armanStandard, 1, "101", "102");
        var armanDeluxeRooms = MakeRooms(armanDeluxe, 2, "201", "202");
        var armanSuiteRooms = MakeRooms(armanSuite, 3, "301");

        var sheratonClassicRooms = MakeRooms(sheratonClassic, 1, "101", "102", "103");
        var sheratonDeluxeRooms = MakeRooms(sheratonDeluxe, 2, "201", "202");
        var sheratonSuiteRooms = MakeRooms(sheratonSuite, 3, "301");

        var baronStandardRooms = MakeRooms(baronStandard, 1, "101", "102");
        var baronDeluxeRooms = MakeRooms(baronDeluxe, 2, "201");

        context.Rooms.AddRange(
            chamSuperiorRooms.Concat(chamDeluxeRooms).Concat(chamSuiteRooms)
            .Concat(damaSuperiorRooms).Concat(damaDeluxeRooms).Concat(damaJuniorSuiteRooms).Concat(damaDeluxeSuiteRooms).Concat(damaExecutiveSuiteRooms).Concat(damaPresidentialSuiteRooms)
            .Concat(fsPremierRooms).Concat(fsExecutiveSuiteRooms).Concat(fsRoyalSuiteRooms).Concat(fsResidenceOneRooms).Concat(fsDiplomaticSuiteRooms)
            .Concat(safirStandardRooms).Concat(safirDeluxeRooms).Concat(safirSuiteRooms)
            .Concat(seaStandardRooms).Concat(seaViewRoomRooms).Concat(seaFamilyRooms)
            .Concat(grandStandardRooms).Concat(grandDeluxeRooms).Concat(grandFamilyRooms)
            .Concat(orientHeritageRooms).Concat(orientDeluxeHeritageRooms)
            .Concat(armanStandardRooms).Concat(armanDeluxeRooms).Concat(armanSuiteRooms)
            .Concat(sheratonClassicRooms).Concat(sheratonDeluxeRooms).Concat(sheratonSuiteRooms)
            .Concat(baronStandardRooms).Concat(baronDeluxeRooms)
        );
        await context.SaveChangesAsync();

        // ─── Bookings ─────────────────────────────────────────
        var bookingsList = new List<Booking>
        {
            // Confirmed - مستقبلي
            MakeBooking(guest1.Id, chamPalace.Id, chamSuperior,
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(13)),
                chamSuperiorRooms[0], BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-3),
                new GuestDetail { FullName = "Ahmad Ali", Nationality = "Saudi", IsPrimary = true }
            ),

            // Confirmed - مستقبلي
            MakeBooking(guest2.Id, damaRose.Id, damaDeluxeSuite,
                DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(24)),
                damaDeluxeSuiteRooms[0], BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-2),
                new GuestDetail { FullName = "Sara Ahmed", Nationality = "Egyptian", IsPrimary = true }
            ),

            // Completed - الشهر الماضي
            MakeBooking(guest1.Id, chamPalace.Id, chamSuite,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-17)),
                chamSuiteRooms[0], BookingStatus.Completed,
                DateTime.UtcNow.AddMonths(-1),
                new GuestDetail { FullName = "Mohammad Hassan", Nationality = "Saudi", IsPrimary = true }
            ),

            // Completed - قبل 3 أشهر
            MakeBooking(guest2.Id, fourSeasons.Id, fsPremier,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-95)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-92)),
                fsPremierRooms[0], BookingStatus.Completed,
                DateTime.UtcNow.AddMonths(-3),
                new GuestDetail { FullName = "Layla Omar", Nationality = "Jordanian", IsPrimary = true }
            ),

            // Cancelled
            MakeBooking(guest1.Id, damaRose.Id, damaSuperior,
                DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(33)),
                damaSuperiorRooms[1], BookingStatus.Cancelled,
                DateTime.UtcNow.AddDays(-5),
                new GuestDetail { FullName = "Ahmad Ali", Nationality = "Saudi", IsPrimary = true }
            ),

            // Pending
            MakeBooking(guest2.Id, safirHoms.Id, safirStandard,
                DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(18)),
                safirStandardRooms[0], BookingStatus.Pending,
                DateTime.UtcNow,
                new GuestDetail { FullName = "Khaled Salem", Nationality = "Kuwaiti", IsPrimary = true }
            ),

            // Confirmed - مستقبلي
            MakeBooking(guest1.Id, sheratonAleppo.Id, sheratonDeluxe,
                DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(9)),
                sheratonDeluxeRooms[0], BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-1),
                new GuestDetail { FullName = "Nour Khalil", Nationality = "Lebanese", IsPrimary = true }
            ),

            // Completed
            MakeBooking(guest2.Id, arman.Id, armanSuite,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-8)),
                armanSuiteRooms[0], BookingStatus.Completed,
                DateTime.UtcNow.AddDays(-12),
                new GuestDetail { FullName = "Rami Youssef", Nationality = "Syrian", IsPrimary = true }
            ),

            // Confirmed - مستقبلي
            MakeBooking(guest1.Id, seaView.Id, seaViewRoom,
                DateOnly.FromDateTime(DateTime.Today.AddDays(25)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(28)),
                seaViewRoomRooms[0], BookingStatus.Confirmed,
                DateTime.UtcNow.AddDays(-4),
                new GuestDetail { FullName = "Hala Zain", Nationality = "Syrian", IsPrimary = true }
            ),

            // Cancelled
            MakeBooking(guest2.Id, grandTartous.Id, grandFamily,
                DateOnly.FromDateTime(DateTime.Today.AddDays(40)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(43)),
                grandFamilyRooms[0], BookingStatus.Cancelled,
                DateTime.UtcNow.AddDays(-6),
                new GuestDetail { FullName = "Fadi Haddad", Nationality = "Syrian", IsPrimary = true }
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
        var hotelViews = new List<HotelView>();

        foreach (var (hotel, maxViews, maxClicks) in new[]
        {
            (chamPalace, 200, 50),
            (damaRose, 180, 45),
            (fourSeasons, 160, 40),
            (sheratonAleppo, 150, 38),
            (safirHoms, 90, 25),
            (grandTartous, 70, 20),
            (seaView, 60, 18),
            (arman, 55, 15),
            (orientHouse, 40, 12),
            (baron, 35, 10)
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
            // Tours & Experiences
            new Partner { Category = "Tours", Name = "Old City Walk", Cities = new() { "Damascus" }, Description = "Explore historic alleys, traditional markets, and guided heritage stops through the heart of Damascus." },
            new Partner { Category = "Tours", Name = "Aleppo Citadel Route", Cities = new() { "Aleppo" }, Description = "A guided trip through landmarks, local food corners, and the iconic citadel experience." },
            new Partner { Category = "Tours", Name = "Desert Sunset Trek", Cities = new() { "Palmyra" }, Description = "A guided desert journey with sunset views, campsite stories, and a calm night under the stars." },
            new Partner { Category = "Tours", Name = "Coastal Sea Activities", Cities = new() { "Latakia", "Tartous" }, Description = "Enjoy boat time, beach relaxation, and water activities led by local guides on the coast." },
            // Car Rentals
            new Partner { Category = "CarRental", Name = "Cham Car Hire", Cities = new() { "Damascus", "Aleppo" }, Description = "Economy and family cars for daily or weekly hire, with branches in Damascus and Aleppo and delivery to your hotel." },
            new Partner { Category = "CarRental", Name = "Coast Auto Rental", Cities = new() { "Latakia" }, Description = "Self-drive rentals along the coast, airport pickup available." },
            // Transport & Transfers
            new Partner { Category = "Transport", Name = "Airport Express Transfers", Cities = new() { "Damascus", "Aleppo", "Latakia" }, Description = "Fixed-price private transfers between the main airports and city hotels." },
            new Partner { Category = "Transport", Name = "Intercity Shuttle", Cities = new() { "Aleppo", "Homs", "Hama" }, Description = "Scheduled shared rides between Aleppo, Homs, and Hama." },
            // Dining
            new Partner { Category = "Dining", Name = "Beit Sitti Restaurant", Cities = new() { "Damascus" }, Description = "Traditional Damascene cuisine in a restored courtyard house, guest discount with your booking receipt." },
            new Partner { Category = "Dining", Name = "Seafront Grill", Cities = new() { "Tartous" }, Description = "Fresh seafood and mezze with a terrace over the marina." }
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
