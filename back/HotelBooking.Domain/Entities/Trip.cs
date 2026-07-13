namespace HotelBooking.Domain.Entities
{
    // CHANGED BY AI (2026-07-13): please review. New entity backing the "Facilities &
    // Attractions" page's trips list — previously 100% localStorage (per-browser, not synced,
    // not really admin-managed content). Difficulty/Type/PriceLabel are kept as free-text rather
    // than enums since the admin picks from a small fixed set already enforced client-side, and
    // this is marketing content rather than a workflow-critical field.
    public class Trip : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PriceLabel { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
