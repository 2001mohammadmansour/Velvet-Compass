namespace HotelBooking.Application.DTOs.Partners
{
    public record PartnerDto(
        long Id,
        string Name,
        List<string> Cities,
        string Description,
        string? ImageUrl,
        string Category,
        string? WebsiteUrl,
        int ClickCount
    );
}
