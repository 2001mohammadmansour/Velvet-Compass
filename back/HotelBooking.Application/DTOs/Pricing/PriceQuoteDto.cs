namespace HotelBooking.Application.DTOs.Pricing
{
    public record PriceQuoteDto(
        List<NightlyPriceDto> Nights,
        decimal Total
    );
}
