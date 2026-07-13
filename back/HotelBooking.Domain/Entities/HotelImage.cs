namespace HotelBooking.Domain.Entities
{
    public class HotelImage : BaseEntity
    {
        public long HotelId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }


        public Hotel Hotel { get; set; } = null!;

    }
}