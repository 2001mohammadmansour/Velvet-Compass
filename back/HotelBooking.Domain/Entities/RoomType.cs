namespace HotelBooking.Domain.Entities
{
    public class RoomType : BaseEntity
    {
        public long HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Beds { get; set; }
        public decimal BasePrice { get; set; }

        public Hotel Hotel { get; set; } = null!;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<RoomTypeImage> RoomTypeImages { get; set; } = new List<RoomTypeImage>();

    }
}
