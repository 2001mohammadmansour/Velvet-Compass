using HotelBooking.Application.DTOs.Amenities;
using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.DTOs.RoomTypes;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class RoomTypeServices : IRoomTypeService
    {
        private readonly AppDbContext _context;
        public RoomTypeServices(AppDbContext context)
        {
            _context = context;
        }

        // CHANGED BY AI (2026-07-15): please review. Backs the homepage's cross-hotel search
        // (previously a mock "/api/rooms/search" endpoint that didn't exist on this backend at
        // all). Returns every room type across every hotel; the frontend still does destination/
        // price/stars/score filtering client-side over this full list, same as before. Now excludes
        // room types with zero rooms available (for the given dates, or overall if no dates were
        // given) and reports AvailableCount so the frontend can show a "few left" notice.
        public async Task<List<RoomSearchResultDto>> SearchAsync(DateOnly? checkIn = null, DateOnly? checkOut = null)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .Include(rt => rt.RoomTypeImages)
                .ToListAsync();

            var roomTypeIds = roomTypes.Select(rt => rt.Id).ToList();
            var reviewStats = await _context.Reviews
                .Where(r => roomTypeIds.Contains(r.RoomTypeId))
                .GroupBy(r => r.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, Avg = g.Average(r => r.OverallScore), Count = g.Count() })
                .ToDictionaryAsync(s => s.RoomTypeId);
            var availableCounts = await GetAvailableCountsAsync(roomTypeIds, checkIn, checkOut);

            return roomTypes
                .Where(rt => availableCounts.GetValueOrDefault(rt.Id, 0) > 0)
                .Select(rt =>
            {
                reviewStats.TryGetValue(rt.Id, out var stats);
                return new RoomSearchResultDto(
                    rt.Id,
                    rt.HotelId,
                    rt.Name,
                    rt.Hotel.Name,
                    rt.Hotel.City,
                    rt.Hotel.Country,
                    rt.Hotel.StarRating,
                    rt.BasePrice,
                    rt.Capacity,
                    rt.RoomTypeImages.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                    stats != null ? Math.Round(stats.Avg, 1) : null,
                    stats?.Count ?? 0,
                    rt.Description,
                    rt.AllowExtraBed,
                    rt.MaxExtraBeds,
                    rt.ExtraBedPriceType.ToString(),
                    rt.ExtraBedPriceForOneBed,
                    rt.ExtraBedPriceForTwoBeds,
                    availableCounts.GetValueOrDefault(rt.Id, 0)
                );
            }).ToList();
        }

        // CHANGED BY AI (2026-07-15): please review. Bulk version of BookingService's
        // GetAvailableRoomsAsync, adapted to report a per-room-type count across many room types at
        // once (2 queries total) instead of checking one room type at a time. Same rules: a room
        // counts as available unless its Status isn't Available, or it has a Booked/Blocked
        // RoomAvailability row overlapping [checkIn, checkOut) (checkout day itself is free). With
        // no dates given, this is just the total Available-status room count per type.
        private async Task<Dictionary<long, int>> GetAvailableCountsAsync(List<long> roomTypeIds, DateOnly? checkIn, DateOnly? checkOut)
        {
            var totalByRoomType = await _context.Rooms
                .Where(r => roomTypeIds.Contains(r.RoomTypeId) && r.Status == RoomStatus.Available)
                .GroupBy(r => r.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.RoomTypeId, g => g.Count);

            if (checkIn is null || checkOut is null || checkOut <= checkIn)
                return totalByRoomType;

            var blockedByRoomType = await _context.RoomAvailabilities
                .Where(a =>
                    a.Date >= checkIn && a.Date < checkOut &&
                    (a.Status == RoomAvailabilityStatus.Booked || a.Status == RoomAvailabilityStatus.Blocked) &&
                    a.Room.Status == RoomStatus.Available &&
                    roomTypeIds.Contains(a.Room.RoomTypeId))
                .Select(a => new { a.RoomId, a.Room.RoomTypeId })
                .Distinct()
                .GroupBy(x => x.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.RoomTypeId, g => g.Count);

            return roomTypeIds.ToDictionary(id => id, id =>
                Math.Max(0, totalByRoomType.GetValueOrDefault(id, 0) - blockedByRoomType.GetValueOrDefault(id, 0)));
        }

        public async Task<RoomTypeDetailDto> GetRoomTypeById(long hotelId, long roomTypeId)
        {
            var roomType = await _context.RoomTypes.
                Include(r => r.RoomTypeImages.OrderBy(r => r.SortOrder)).
                // CHANGED BY AI (2026-07-15): please review. Needed so MapToRoomTypeDetailDto can
                // populate the new Amenities field.
                Include(r => r.RoomTypeAmenities).ThenInclude(rta => rta.Amenity).
                FirstOrDefaultAsync(r => r.Id == roomTypeId && r.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            return MapToRoomTypeDetailDto(roomType);

        }
        public async Task<List<RoomTypeSummaryDto>> GetRoomTypesByHotelAsync(long hotelId, DateOnly? checkIn = null, DateOnly? checkOut = null)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.RoomTypeImages)
                // CHANGED BY AI (2026-07-15): please review. Needed for the Amenities field below;
                // a hotel typically has only a handful of room types, so this join is cheap (same
                // reasoning already applied to the review-stats query just below).
                .Include(rt => rt.RoomTypeAmenities).ThenInclude(rta => rta.Amenity)
                .Where(rt => rt.HotelId == hotelId)
                .OrderBy(rt => rt.CreatedAt)
                .ToListAsync();

            // CHANGED BY AI (2026-07-13): please review. New review-stats join for the Reviews
            // feature — a hotel typically has only a handful of room types, so grouping in a
            // second small query (rather than a single combined query) keeps this readable without
            // meaningfully hurting performance at this scale.
            var roomTypeIds = roomTypes.Select(rt => rt.Id).ToList();
            var reviewStats = await _context.Reviews
                .Where(r => roomTypeIds.Contains(r.RoomTypeId))
                .GroupBy(r => r.RoomTypeId)
                .Select(g => new { RoomTypeId = g.Key, Avg = g.Average(r => r.OverallScore), Count = g.Count() })
                .ToDictionaryAsync(s => s.RoomTypeId);
            // CHANGED BY AI (2026-07-15): please review. Same sold-out exclusion + "few left" count
            // as the cross-hotel search below, reusing the same bulk availability query.
            var availableCounts = await GetAvailableCountsAsync(roomTypeIds, checkIn, checkOut);

            return roomTypes
                .Where(rt => availableCounts.GetValueOrDefault(rt.Id, 0) > 0)
                .Select(rt =>
            {
                reviewStats.TryGetValue(rt.Id, out var stats);
                return new RoomTypeSummaryDto(
                    rt.Id,
                    rt.Name,
                    rt.Capacity,
                    rt.Beds,
                    rt.BasePrice,
                    rt.RoomTypeImages.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                    stats != null ? Math.Round(stats.Avg, 1) : null,
                    stats?.Count ?? 0,
                    rt.Description,
                    rt.RoomTypeAmenities.Select(rta => new AmenityDto(rta.Amenity.Id, rta.Amenity.Name, rta.Amenity.Icon, rta.Amenity.Scope.ToString(), rta.Amenity.IsActive)).ToList(),
                    rt.AllowExtraBed,
                    rt.MaxExtraBeds,
                    rt.ExtraBedPriceType.ToString(),
                    rt.ExtraBedPriceForOneBed,
                    rt.ExtraBedPriceForTwoBeds,
                    availableCounts.GetValueOrDefault(rt.Id, 0)
                );
            }).ToList();
        }
        public async Task<RoomTypeDetailDto> CreateAsync(long ownerId, long hotelId, CreateRoomTypeRequest request)
        {
            await VerifyHotelOwnershipAsync(ownerId, false, hotelId);
            var (allowExtraBed, maxExtraBeds, priceType, priceForOne, priceForTwo) = ValidateExtraBedSettings(
                request.AllowExtraBed, request.MaxExtraBeds, request.ExtraBedPriceType, request.ExtraBedPriceForOneBed, request.ExtraBedPriceForTwoBeds);
            var roomType = new RoomType
            {
                HotelId = hotelId,
                Name = request.Name,
                Description = request.Description,
                Capacity = request.Capacity,
                Beds = request.Beds,
                BasePrice = request.BasePrice,
                AllowExtraBed = allowExtraBed,
                MaxExtraBeds = maxExtraBeds,
                ExtraBedPriceType = priceType,
                ExtraBedPriceForOneBed = priceForOne,
                ExtraBedPriceForTwoBeds = priceForTwo
            };
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return MapToRoomTypeDetailDto(roomType);

        }
        public async Task<RoomTypeDetailDto> UpdateAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, UpdateRoomTypeRequest request)
        {
            await VerifyHotelOwnershipAsync(callerId, isAdmin, hotelId);
            var roomType = await _context.RoomTypes
                .Include(rt => rt.RoomTypeImages)
                .Include(rt => rt.RoomTypeAmenities).ThenInclude(rta => rta.Amenity)
                .FirstOrDefaultAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            var (allowExtraBed, maxExtraBeds, priceType, priceForOne, priceForTwo) = ValidateExtraBedSettings(
                request.AllowExtraBed, request.MaxExtraBeds, request.ExtraBedPriceType, request.ExtraBedPriceForOneBed, request.ExtraBedPriceForTwoBeds);

            roomType.Name = request.Name;
            roomType.Description = request.Description;
            roomType.Capacity = request.Capacity;
            roomType.Beds = request.Beds;
            roomType.BasePrice = request.BasePrice;
            roomType.AllowExtraBed = allowExtraBed;
            roomType.MaxExtraBeds = maxExtraBeds;
            roomType.ExtraBedPriceType = priceType;
            roomType.ExtraBedPriceForOneBed = priceForOne;
            roomType.ExtraBedPriceForTwoBeds = priceForTwo;

            await _context.SaveChangesAsync();
            return MapToRoomTypeDetailDto(roomType);
        }

        // CHANGED BY AI (2026-07-15): please review. Validates the extra-bed system's create/update
        // input: MaxExtraBeds must be 0/1/2, prices must be non-negative, and MaxExtraBeds is
        // forced to 0 whenever AllowExtraBed is false regardless of what was sent (defensive,
        // mirrors how breakfast add-ons are ignored when the hotel doesn't offer them).
        private static (bool AllowExtraBed, int MaxExtraBeds, ExtraBedPriceType PriceType, decimal PriceForOne, decimal PriceForTwo) ValidateExtraBedSettings(
            bool allowExtraBed, int maxExtraBeds, string priceType, decimal priceForOne, decimal priceForTwo)
        {
            if (!allowExtraBed)
                return (false, 0, ExtraBedPriceType.Percentage, 0m, 0m);

            if (maxExtraBeds is not (1 or 2))
                throw new InvalidRoomTypeConfigurationException("MaxExtraBeds must be 1 or 2 when extra beds are allowed.");

            if (priceForOne < 0 || priceForTwo < 0)
                throw new InvalidRoomTypeConfigurationException("Extra-bed prices cannot be negative.");

            ExtraBedPriceType parsedType;
            try
            {
                parsedType = Enum.Parse<ExtraBedPriceType>(priceType, ignoreCase: true);
            }
            catch (Exception)
            {
                throw new InvalidRoomTypeConfigurationException($"Invalid extra-bed price type '{priceType}'.");
            }

            return (true, maxExtraBeds, parsedType, priceForOne, priceForTwo);
        }
        // CHANGED BY AI (2026-07-13): please review. Correcting an earlier wrong diagnosis here:
        // the delete failure was NOT caused by Room -> RoomType (that's already Cascade, so its
        // Rooms and their RoomAvailabilities delete cleanly). The actual blocker is
        // BookingItem -> RoomType, which is Restrict (by design, to preserve booking/financial
        // history) — so a room type that has ever been booked can't be removed. Rather than
        // cascade-deleting real booking records (destructive) or leaving a generic crash, this
        // now checks for booking history up front and returns a clear, specific error instead.
        public async Task DeleteAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId)
        {
            await VerifyHotelOwnershipAsync(callerId, isAdmin, hotelId);
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            var hasBookings = await _context.BookingItems.AnyAsync(bi => bi.RoomTypeId == roomTypeId);
            if (hasBookings)
                throw new RoomTypeHasBookingsException();

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
        }
        public async Task AddImageAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, AddRoomTypeImageRequest request)
        {
            await VerifyHotelOwnershipAsync(callerId, isAdmin, hotelId);
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);
            if (request.IsPrimary)
            {
                // CHANGED BY AI (2026-07-13): please review — was filtering on rti.Id (the image's
                // own id) instead of rti.RoomTypeId, so it cleared the wrong row's IsPrimary flag
                // instead of the room type's previous primary image.
                var existing = await _context.RoomTypeImages
                    .Where(rti => rti.RoomTypeId == roomTypeId && rti.IsPrimary)
                    // this instead of ForEach
                    .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsPrimary, false));

                //existing.ForEach(rti => rti.IsPrimary = false);
            }

            var maxOrder = await _context.RoomTypeImages
                .Where(rti => rti.RoomTypeId == roomTypeId)
                .MaxAsync(rti => (int?)rti.SortOrder) ?? 0;

            _context.RoomTypeImages.Add(new RoomTypeImage
            {
                RoomTypeId = roomTypeId,
                Url = request.Url,
                Caption = request.Caption,
                IsPrimary = request.IsPrimary,
                SortOrder = (maxOrder + 1)
            });
            await _context.SaveChangesAsync();

        }
        public async Task DeleteImageAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long imageId)
        {
            await VerifyHotelOwnershipAsync(callerId, isAdmin, hotelId);
            var image = await _context.RoomTypeImages
                .FirstOrDefaultAsync(rti => rti.Id == imageId && rti.RoomTypeId == roomTypeId)
                ?? throw new Exception("Image Not Found");

            _context.RoomTypeImages.Remove(image);
            await _context.SaveChangesAsync();

        }
        private async Task VerifyHotelOwnershipAsync(long callerId, bool isAdmin, long hotelId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();
        }

        // CHANGED BY AI (2026-07-15): please review. Lets an Owner/Admin set a room type's full set
        // of amenities (sea view, minibar, balcony, etc.) in one call — full-replace semantics,
        // same tolerant-of-invalid-ids approach as HotelServices.SetHotelAmenitiesAsync.
        public async Task SetRoomTypeAmenitiesAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, List<long> amenityIds)
        {
            await VerifyHotelOwnershipAsync(callerId, isAdmin, hotelId);
            var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId);
            if (!roomTypeExists)
                throw new RoomTypeNotFoundException(roomTypeId);

            var validIds = await _context.Amenities
                .Where(a => amenityIds.Contains(a.Id) && a.IsActive && a.Scope == AmenityScope.RoomType)
                .Select(a => a.Id)
                .ToListAsync();

            var existing = _context.RoomTypeAmenities.Where(rta => rta.RoomTypeId == roomTypeId);
            _context.RoomTypeAmenities.RemoveRange(existing);
            foreach (var id in validIds)
                _context.RoomTypeAmenities.Add(new RoomTypeAmenity { RoomTypeId = roomTypeId, AmenityId = id });

            await _context.SaveChangesAsync();
        }

        private static RoomTypeDetailDto MapToRoomTypeDetailDto(RoomType roomType) => new(
            roomType.Id,
            roomType.HotelId,
            roomType.Name,
            roomType.Description,
            roomType.Capacity,
            roomType.Beds,
            roomType.BasePrice,
            roomType.CreatedAt,
            roomType.RoomTypeImages
            .OrderBy(r => r.SortOrder)
            .Select(i => new RoomTypeImageDto(i.Id, i.Url, i.Caption, i.IsPrimary, i.SortOrder))
            .ToList(),
            roomType.RoomTypeAmenities
            .Select(rta => new AmenityDto(rta.Amenity.Id, rta.Amenity.Name, rta.Amenity.Icon, rta.Amenity.Scope.ToString(), rta.Amenity.IsActive))
            .ToList(),
            roomType.AllowExtraBed,
            roomType.MaxExtraBeds,
            roomType.ExtraBedPriceType.ToString(),
            roomType.ExtraBedPriceForOneBed,
            roomType.ExtraBedPriceForTwoBeds
            );


    }
}
