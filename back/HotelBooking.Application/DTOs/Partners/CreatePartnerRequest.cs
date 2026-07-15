namespace HotelBooking.Application.DTOs.Partners
{
    public record CreatePartnerRequest(
        string Name,
        string City,
        string Description
    );
}
