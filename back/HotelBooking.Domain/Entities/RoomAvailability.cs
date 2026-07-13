using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    public class RoomAvailability : BaseEntity
    {
        public long RoomId { get; set; }
        public DateOnly Date { get; set; }
        public RoomAvailabilityStatus Status { get; set; } = RoomAvailabilityStatus.Free;
        public decimal? PriceOverride { get; set; }
        public Room Room { get; set; } = null!;
    }
}
