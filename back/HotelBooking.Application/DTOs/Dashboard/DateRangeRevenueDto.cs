namespace HotelBooking.Application.DTOs.Dashboard;

public record DateRangeRevenueRequest(
    long HotelId,
    DateOnly From,
    DateOnly To
);

public record DateRangeRevenueDto(
    DateOnly From,
    DateOnly To,
    decimal GrossRevenue,
    decimal PlatformFee,
    decimal NetRevenue,
    int BookingsCount,
    int CancelledCount
);

// ─── Reservations calendar ────────────────────────────────────────────────
// One physical room manually taken out of service (Blocked) on a given day.
public record CalendarBlockedRoomDto(string RoomNumber, string RoomTypeName);

// One room type's unit counts on a single calendar day.
public record CalendarRoomTypeDayDto(
    long RoomTypeId,
    string RoomTypeName,
    int TotalUnits,
    int OccupiedUnits,   // Σ qty of confirmed/completed bookings whose stay covers this night
    int BlockedUnits,    // physical rooms of this type blocked this day
    int AvailableUnits   // TotalUnits − OccupiedUnits − BlockedUnits (floored at 0)
);

public record CalendarDayDto(
    DateOnly Date,
    int TotalUnits,
    int OccupiedUnits,
    int BlockedUnits,
    int AvailableUnits,
    List<CalendarRoomTypeDayDto> RoomTypes,
    List<CalendarBlockedRoomDto> BlockedRooms
);

// One room type's performance over a date window (attributed by check-in date). Room types with
// no activity in the window are still returned as zero rows.
public record RoomPerformanceDto(
    string RoomType,
    int Booked,            // confirmed/completed booking lines for this room type
    int RoomNights,        // Σ(qty × nights)
    int Cancelled,         // cancelled booking lines
    double CancelRate,     // cancelled ÷ (booked + cancelled), 0–1
    decimal Revenue,       // Σ(line total) for confirmed/completed
    double RevenueShare,   // this room's revenue ÷ all rooms' revenue, 0–1
    decimal Adr,           // revenue ÷ room-nights
    double AvgStay         // average nights per booking line
);
