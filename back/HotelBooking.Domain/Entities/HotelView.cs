namespace HotelBooking.Domain.Entities;

public class HotelView : BaseEntity
{
    public long HotelId { get; set; }
    public DateOnly Date { get; set; }
    public int Views { get; set; } = 0;
    public int Clicks { get; set; } = 0;

    // Navigation
    public Hotel Hotel { get; set; } = null!;
}
