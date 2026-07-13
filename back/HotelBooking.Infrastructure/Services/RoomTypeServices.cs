using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.DTOs.RoomTypes;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
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

        // CHANGED BY AI (2026-07-13): please review. Backs the homepage's cross-hotel search
        // (previously a mock "/api/rooms/search" endpoint that didn't exist on this backend at
        // all). Returns every room type across every hotel; the frontend still does destination/
        // price/stars/score filtering client-side over this full list, same as before. No
        // date-based availability filtering — matches the existing single-hotel room listing,
        // which has the same limitation.
        public async Task<List<RoomSearchResultDto>> SearchAsync()
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

            return roomTypes.Select(rt =>
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
                    stats?.Count ?? 0
                );
            }).ToList();
        }

        public async Task<RoomTypeDetailDto> GetRoomTypeById(long hotelId, long roomTypeId)
        {
            var roomType = await _context.RoomTypes.
                Include(r => r.RoomTypeImages.OrderBy(r => r.SortOrder)).
                FirstOrDefaultAsync(r => r.Id == roomTypeId && r.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            return MapToRoomTypeDetailDto(roomType);

        }
        public async Task<List<RoomTypeSummaryDto>> GetRoomTypesByHotelAsync(long hotelId)
        {
            var roomTypes = await _context.RoomTypes
                .Include(rt => rt.RoomTypeImages)
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

            return roomTypes.Select(rt =>
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
                    stats?.Count ?? 0
                );
            }).ToList();
        }
        public async Task<RoomTypeDetailDto> CreateAsync(long ownerId, long hotelId, CreateRoomTypeRequest request)
        {
            await VerifyHotelOwnershipAsync(ownerId, false, hotelId);
            var roomType = new RoomType
            {
                HotelId = hotelId,
                Name = request.Name,
                Description = request.Description,
                Capacity = request.Capacity,
                Beds = request.Beds,
                BasePrice = request.BasePrice
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
                .FirstOrDefaultAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId)
                ?? throw new RoomTypeNotFoundException(roomTypeId);

            roomType.Name = request.Name;
            roomType.Description = request.Description;
            roomType.Capacity = request.Capacity;
            roomType.Beds = request.Beds;
            roomType.BasePrice = request.BasePrice;

            await _context.SaveChangesAsync();
            return MapToRoomTypeDetailDto(roomType);
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
            .ToList()
            );


    }
}
