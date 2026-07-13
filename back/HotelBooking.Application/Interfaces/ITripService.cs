using HotelBooking.Application.DTOs.Trips;

namespace HotelBooking.Application.Interfaces
{
    public interface ITripService
    {
        Task<List<TripDto>> GetAllAsync();
        Task<TripDto> CreateAsync(CreateTripRequest request);
        Task<TripDto> UpdateAsync(long tripId, UpdateTripRequest request);
        Task DeleteAsync(long tripId);
    }
}
