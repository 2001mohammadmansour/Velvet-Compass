namespace HotelBooking.Domain.Entities
{
    public class BookingItem : BaseEntity
    {
        public long BookingId { get; set; }
        public long RoomTypeId { get; set; }
        public long? RoomId { get; set; }
        public int Nights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public int Qty { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
        public RoomType RoomType { get; set; } = null!;
        public Room? Room { get; set; }
    }
}
