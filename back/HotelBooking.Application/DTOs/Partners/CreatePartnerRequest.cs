namespace HotelBooking.Application.DTOs.Partners
{
    public record CreatePartnerRequest(
        string Name,
        List<string> Cities,
        string Description,
        string Category,
        string? WebsiteUrl
    );
}
