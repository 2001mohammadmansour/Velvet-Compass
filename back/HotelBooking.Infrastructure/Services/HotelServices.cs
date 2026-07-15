using HotelBooking.Application.DTOs.Amenities;
using HotelBooking.Application.DTOs.Common;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class HotelServices : IHotelService
    {

        private readonly AppDbContext _context;
        // CHANGED BY AI (2026-07-15): please review. Used to pre-populate a new hotel with default
        // seasonal/occupancy pricing rules right after creation — see CreateHotelAsync below.
        // Pricing rules moved from per-room-type to hotel scope, so seeding now happens once per
        // hotel instead of once per room type.
        private readonly IHotelPricingService _pricingService;

        public HotelServices(AppDbContext context, IHotelPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        public async Task<PagedResult<HotelSummaryDto>> GetAllAsync(HotelFilterRequest filterRequest)
        {
            var query = _context.Hotels
                .Include(x => x.HotelImages)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterRequest.City))
                query = query.Where(h => h.City.ToLower().Contains(filterRequest.City.ToLower()));

            if (!string.IsNullOrWhiteSpace(filterRequest.Country))
                query = query.Where(h => h.Country.ToLower().Contains(filterRequest.Country.ToLower()));

            if (filterRequest.MinStars.HasValue)
                query = query.Where(h => h.StarRating >= filterRequest.MinStars.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip((filterRequest.Page - 1) * filterRequest.PageSize)
                .Take(filterRequest.PageSize)
                .Select(h => new HotelSummaryDto(
                      h.Id,
                      h.Name,
                      h.City,
                      h.Country,
                      h.StarRating,
                    h.HotelImages.Where(h => h.IsPrimary).Select(i => i.Url).FirstOrDefault()
                    ))
                .ToListAsync();
            return new PagedResult<HotelSummaryDto>(items, totalCount, filterRequest.Page, filterRequest.PageSize);
        }
        public async Task<HotelDetailDto> GetByIdAsync(long hotelId)
        {
            var hotel = await _context.Hotels
                .Include(h => h.HotelImages.OrderBy(h => h.SortOrder))
                // CHANGED BY AI (2026-07-15): please review. Needed so MapToHotelDetailDto can
                // populate the new Amenities field.
                .Include(h => h.HotelAmenities).ThenInclude(ha => ha.Amenity)
                .FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            return MapToHotelDetailDto(hotel);

        }
        // CHANGED BY AI (2026-07-12): please review. Previously this took the caller's own id as
        // ownerId, which meant an Admin calling this endpoint would try to make themselves the
        // owner and then fail the role check below (Admin != Owner) — so hotel creation could
        // never actually succeed. Now the target owner is looked up by request.OwnerEmail, so an
        // Admin can create a hotel on behalf of a specific pending owner.
        public async Task<HotelDetailDto> CreateHotelAsync(CreateHotelRequest request)
        {
            var existeOwner = _context.Users.FirstOrDefault(u => u.Email == request.OwnerEmail);
            if (existeOwner is null)
                throw new OwnerNotFoundException(request.OwnerEmail);

            if (existeOwner.Role != UserRole.Owner)
                throw new Exception("Only users with the Owner role can create and own a hotel.");
            var hotel = new Hotel
            {
                OwnerId = existeOwner.Id,
                Name = request.Name,
                Description = request.Description,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                StarRating = request.StarRating,
                Phone = request.Phone,
                Email = request.Email,
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-15): please review. Pre-populates sensible pricing defaults
            // (a seasonal calendar + occupancy surge tiers) so the owner doesn't start from a
            // blank slate; these are ordinary rows they can edit or delete afterward.
            await _pricingService.SeedDefaultsAsync(hotel.Id, DateOnly.FromDateTime(DateTime.UtcNow));

            return MapToHotelDetailDto(hotel);
        }
        public async Task<HotelDetailDto> UpdateHotelAsync(long callerId, bool isAdmin, long HotelId, UpdateHotelRequest request)
        {

            var hotel = await GetOwnedHotelAsync(callerId, isAdmin, HotelId);

            hotel.Name = request.Name;
            hotel.Description = request.Description;
            hotel.Address = request.Address;
            hotel.City = request.City;
            hotel.Country = request.Country;
            hotel.StarRating = request.StarRating;
            hotel.Phone = request.Phone;
            hotel.Email = request.Email;

            await _context.SaveChangesAsync();
            return MapToHotelDetailDto(hotel);

        }
        public async Task DeleteHotelAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await GetOwnedHotelAsync(callerId, isAdmin, hotelId);
            _context.Hotels.Remove(hotel);
            _context.SaveChanges();
        }
        public async Task AddImagesAsync(long callerId, bool isAdmin, long hotelId, string url, string caption, bool IsPrimary)
        {
            var hotel = GetOwnedHotelAsync(callerId, isAdmin, hotelId);


            if (IsPrimary)
            {
                var existingImage = await _context.HotelImages
                    .Where(i => i.HotelId == hotelId && i.IsPrimary)
                    .ToListAsync();
                existingImage.ForEach(i => i.IsPrimary = false);
            }

            var maxOrder = await _context.HotelImages
                .Where(i => i.HotelId == hotelId)
                .MaxAsync(i => (int?)i.SortOrder) ?? 0;

            var image = new HotelImage
            {
                HotelId = hotelId,
                Url = url,
                Caption = caption,
                IsPrimary = IsPrimary,
                SortOrder = maxOrder + 1,
            };
            _context.HotelImages.Add(image);
            await _context.SaveChangesAsync();

        }
        public async Task DeleteImageAsync(long callerId, bool isAdmin, long hotelId, long imageId)
        {
            var hotel = GetOwnedHotelAsync(callerId, isAdmin, hotelId);
            var image = await _context.HotelImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.HotelId == hotelId)
                ?? throw new Exception($"Image did not found");

            _context.HotelImages.Remove(image);
            await _context.SaveChangesAsync();
        }
        public async Task<List<HotelSummaryDto>> GetMyHotelsAsync(long ownerId)
        {
            return await _context.Hotels
                .Include(i => i.HotelImages)
                .Where(h => h.OwnerId == ownerId)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HotelSummaryDto(
                    h.Id,
                    h.Name,
                    h.City,
                    h.Country,
                    h.StarRating,
                    h.HotelImages.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault()
                    ))
                .ToListAsync();

        }

        //public async Task<HotelDetailDto> GetMyHotelDetailsAsync(long ownerId)
        //{
        //}

        private async Task<Hotel> GetOwnedHotelAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.Include(h => h.HotelImages)
                // CHANGED BY AI (2026-07-15): please review. Needed so callers that map the result
                // to HotelDetailDto (e.g. UpdateHotelAsync) get the Amenities field populated.
                .Include(h => h.HotelAmenities).ThenInclude(ha => ha.Amenity)
                .FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            return hotel;
        }

        // CHANGED BY AI (2026-07-12): please review. Lets an Owner/Admin toggle whether new
        // bookings for this hotel start Confirmed automatically instead of Pending.
        public async Task SetAutoAcceptBookingsAsync(long callerId, bool isAdmin, long hotelId, bool enabled)
        {
            var hotel = await GetOwnedHotelAsync(callerId, isAdmin, hotelId);
            hotel.AutoAcceptBookings = enabled;
            await _context.SaveChangesAsync();
        }

        // CHANGED BY AI (2026-07-12): please review. Lets an Owner/Admin toggle the breakfast
        // add-on and its per-guest-per-night price.
        public async Task SetBreakfastSettingsAsync(long callerId, bool isAdmin, long hotelId, bool available, decimal price)
        {
            var hotel = await GetOwnedHotelAsync(callerId, isAdmin, hotelId);
            hotel.BreakfastAvailable = available;
            hotel.BreakfastPrice = price;
            await _context.SaveChangesAsync();
        }

        // CHANGED BY AI (2026-07-13): please review. Lets an Owner/Admin configure the real
        // cancellation policy (previously the dashboard's "free cancellation" toggle posted to a
        // mock endpoint and never affected anything real).
        public async Task SetCancellationPolicyAsync(long callerId, bool isAdmin, long hotelId, bool freeCancellationEnabled, int freeCancellationDaysBefore, string cancellationFeeType, decimal cancellationFeeValue)
        {
            var hotel = await GetOwnedHotelAsync(callerId, isAdmin, hotelId);
            hotel.FreeCancellationEnabled = freeCancellationEnabled;
            hotel.FreeCancellationDaysBefore = Math.Max(0, freeCancellationDaysBefore);
            hotel.CancellationFeeType = Enum.Parse<CancellationFeeType>(cancellationFeeType, ignoreCase: true);
            hotel.CancellationFeeValue = Math.Max(0, cancellationFeeValue);
            await _context.SaveChangesAsync();
        }

        // CHANGED BY AI (2026-07-15): please review. Lets an Owner/Admin set the hotel's full set
        // of amenities (wifi, parking, gym, etc.) in one call — full-replace semantics. Invalid,
        // inactive, or wrong-scope ids are silently dropped rather than erroring, matching how the
        // rest of this service tolerates a stale/imperfect client.
        public async Task SetHotelAmenitiesAsync(long callerId, bool isAdmin, long hotelId, List<long> amenityIds)
        {
            await GetOwnedHotelAsync(callerId, isAdmin, hotelId);

            var validIds = await _context.Amenities
                .Where(a => amenityIds.Contains(a.Id) && a.IsActive && a.Scope == AmenityScope.Hotel)
                .Select(a => a.Id)
                .ToListAsync();

            var existing = _context.HotelAmenities.Where(ha => ha.HotelId == hotelId);
            _context.HotelAmenities.RemoveRange(existing);
            foreach (var id in validIds)
                _context.HotelAmenities.Add(new HotelAmenity { HotelId = hotelId, AmenityId = id });

            await _context.SaveChangesAsync();
        }

        private static HotelDetailDto MapToHotelDetailDto(Hotel hotel) => new(
            hotel.Id,
            hotel.Name,
            hotel.Description,
            hotel.Address,
            hotel.City,
            hotel.Country,
            hotel.StarRating,
            hotel.Phone,
            hotel.Email,
            hotel.CreatedAt,
            hotel.HotelImages
                .OrderBy(i => i.SortOrder)
                .Select(i => new RoomTypeImageDto(i.Id, i.Url, i.Caption, i.IsPrimary, i.SortOrder))
                .ToList(),
            hotel.AutoAcceptBookings,
            hotel.BreakfastAvailable,
            hotel.BreakfastPrice,
            hotel.FreeCancellationEnabled,
            hotel.FreeCancellationDaysBefore,
            hotel.CancellationFeeType.ToString(),
            hotel.CancellationFeeValue,
            hotel.HotelAmenities
                .Select(ha => new AmenityDto(ha.Amenity.Id, ha.Amenity.Name, ha.Amenity.Icon, ha.Amenity.Scope.ToString(), ha.Amenity.IsActive))
                .ToList()
            );


    }
}
