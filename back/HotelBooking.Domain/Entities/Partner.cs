namespace HotelBooking.Domain.Entities
{
    public class Partner : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // A partner can operate in several cities (e.g. a car-rental firm with branches in
        // Damascus and Aleppo). Stored as a JSON array column; the partner shows under every
        // listed city.
        public List<string> Cities { get; set; } = new();

        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Free-form category slug ("CarRental", "Dining", "Tours", "Transport", "Other") used to
        // group partners into sections on the public page. Kept as a string rather than an enum
        // so admins can introduce a new section without a schema change.
        public string Category { get; set; } = "Other";

        // Optional external site the partner card links out to.
        public string? WebsiteUrl { get; set; }

        // Incremented each time a visitor opens a partner (for the admin "most-viewed" stat).
        public int ClickCount { get; set; }
    }
}
