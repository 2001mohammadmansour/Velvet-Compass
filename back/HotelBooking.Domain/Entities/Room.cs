using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    public class Room : BaseEntity
    {
        public long RoomTypeId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int? Floor { get; set; }
        public string? Notes { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public RoomType RoomType { get; set; } = null!;
        public ICollection<RoomAvailability> Availabilities { get; set; } = new List<RoomAvailability>();
    }
}
