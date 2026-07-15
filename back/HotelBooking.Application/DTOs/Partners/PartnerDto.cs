namespace HotelBooking.Application.DTOs.Partners
{
    public record PartnerDto(
        long Id,
        string Name,
        string City,
        string Description,
        string? ImageUrl
    );
}
