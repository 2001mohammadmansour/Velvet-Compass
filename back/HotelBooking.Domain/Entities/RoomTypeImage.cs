namespace HotelBooking.Domain.Entities
{
    public class RoomTypeImage : BaseEntity
    {
        public long RoomTypeId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public RoomType RoomType { get; set; } = null!;
    }
}
