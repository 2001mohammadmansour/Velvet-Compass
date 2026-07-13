using HotelBooking.Application.DTOs.Trips;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-13): please review. New service backing the "Facilities &
    // Attractions" trips list — previously 100% localStorage (per-browser, not admin-managed for
    // real). Reads are public; writes are Admin-only (enforced in TripsController).
    public class TripService : ITripService
    {
        private readonly AppDbContext _context;
        public TripService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TripDto>> GetAllAsync()
        {
            return await _context.Trips
                .OrderBy(t => t.CreatedAt)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        public async Task<TripDto> CreateAsync(CreateTripRequest request)
        {
            var trip = new Trip
            {
                Title = request.Title,
                City = request.City,
                Price = request.Price,
                PriceLabel = request.PriceLabel,
                Type = request.Type,
                Duration = request.Duration,
                Difficulty = request.Difficulty,
                Description = request.Description,
            };
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateAsync(long tripId, UpdateTripRequest request)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new TripNotFoundException(tripId);

            trip.Title = request.Title;
            trip.City = request.City;
            trip.Price = request.Price;
            trip.PriceLabel = request.PriceLabel;
            trip.Type = request.Type;
            trip.Duration = request.Duration;
            trip.Difficulty = request.Difficulty;
            trip.Description = request.Description;

            await _context.SaveChangesAsync();
            return MapToDto(trip);
        }

        public async Task DeleteAsync(long tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new TripNotFoundException(tripId);

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
        }

        private static TripDto MapToDto(Trip t) => new(
            t.Id, t.Title, t.City, t.Price, t.PriceLabel, t.Type, t.Duration, t.Difficulty, t.Description
        );
    }
}
