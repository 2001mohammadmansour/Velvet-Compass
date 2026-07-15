namespace HotelBooking.Application.DTOs.Partners
{
    public record UpdatePartnerRequest(
        string Name,
        string City,
        string Description
    );
}
