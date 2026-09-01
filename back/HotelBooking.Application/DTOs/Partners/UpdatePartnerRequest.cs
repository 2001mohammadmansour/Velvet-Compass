namespace HotelBooking.Application.DTOs.Partners
{
    public record UpdatePartnerRequest(
        string Name,
        List<string> Cities,
        string Description,
        string Category,
        string? WebsiteUrl
    );
}
