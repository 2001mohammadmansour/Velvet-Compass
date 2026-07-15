using HotelBooking.Application.DTOs.Amenities;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    // CHANGED BY AI (2026-07-15): please review. Admin-managed amenity catalog. Amenities are
    // never hard-deleted (see DeactivateAsync) — deactivating just hides them from picklists
    // without breaking hotels/room types that already reference them.
    public class AmenityService : IAmenityService
    {
        private readonly AppDbContext _context;
        public AmenityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AmenityDto>> GetByScopeAsync(string scope)
        {
            var parsedScope = Enum.Parse<AmenityScope>(scope, ignoreCase: true);
            return await _context.Amenities
                .Where(a => a.IsActive && a.Scope == parsedScope)
                .OrderBy(a => a.Name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<List<AmenityDto>> GetAllByScopeAsync(string scope)
        {
            var parsedScope = Enum.Parse<AmenityScope>(scope, ignoreCase: true);
            return await _context.Amenities
                .Where(a => a.Scope == parsedScope)
                .OrderBy(a => a.Name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<AmenityDto> CreateAsync(CreateAmenityRequest request)
        {
            var scope = Enum.Parse<AmenityScope>(request.Scope, ignoreCase: true);
            var amenity = new Amenity
            {
                Name = request.Name.Trim(),
                Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
                Scope = scope,
                IsActive = true
            };
            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();
            return MapToDto(amenity);
        }

        public async Task<AmenityDto> UpdateAsync(long amenityId, UpdateAmenityRequest request)
        {
            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.Id == amenityId)
                ?? throw new AmenityNotFoundException(amenityId);

            amenity.Name = request.Name.Trim();
            amenity.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
            amenity.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return MapToDto(amenity);
        }

        public async Task DeactivateAsync(long amenityId)
        {
            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.Id == amenityId)
                ?? throw new AmenityNotFoundException(amenityId);

            amenity.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(long amenityId)
        {
            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.Id == amenityId)
                ?? throw new AmenityNotFoundException(amenityId);

            amenity.IsActive = true;
            await _context.SaveChangesAsync();
        }

        private static AmenityDto MapToDto(Amenity a) => new(a.Id, a.Name, a.Icon, a.Scope.ToString(), a.IsActive);
    }
}
