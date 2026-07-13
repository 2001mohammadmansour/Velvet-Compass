using HotelBooking.Application.DTOs.Rooms;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class RoomServices : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomDto>> GetAllByHotelAndTypeAsync
            (long callerId, bool isAdmin, long hotelId, long roomTypeId)
        {
            await VerifyOwnershipAsync(callerId, isAdmin, hotelId, roomTypeId);

            return await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new RoomDto(

                r.Id, r.RoomTypeId, r.RoomNumber,
                r.Floor, r.Notes, r.Status.ToString()
                ))
                .ToListAsync();

        }

        public async Task<List<RoomDto>> GetAllByHotelAsync
            (long callerId, bool isAdmin, long hotelId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.RoomType.HotelId == hotelId)
                .OrderBy(r => r.RoomType.Name)
                .ThenBy(r => r.RoomNumber)
                .Select(r => new RoomDto(
                    r.Id,
                    r.RoomTypeId,
                    r.RoomNumber,
                    r.Floor,
                    r.Notes,
                    r.Status.ToString()
                ))
                .ToListAsync();
        }
        public async Task<RoomDto> CreateAsync(long ownerId, long hotelId, long roomTypeId, CreateRoomRequest request)
        {
            await VerifyOwnershipAsync(ownerId, false, hotelId, roomTypeId);
            var exists = await _context.Rooms
                .AnyAsync(r => r.RoomTypeId == roomTypeId && r.RoomNumber == request.RoomNumber);

            if (exists)
                throw new Exception($"Room number '{request.RoomNumber}' already exists in this room type.");

            var room = new Room
            {
                RoomTypeId = roomTypeId,
                RoomNumber = request.RoomNumber,
                Floor = request.Floor,
                Notes = request.Notes,
                Status = RoomStatus.Available
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return MapToDto(room);
        }
        public async Task DeleteAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId)
        {
            await VerifyOwnershipAsync(callerId, isAdmin, hotelId, roomTypeId);
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.RoomTypeId == roomTypeId)
                ?? throw new RoomNotFoundException(roomId);

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
        public async Task<RoomDto> UpdateAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId, UpdateRoomRequest request)
        {
            await VerifyOwnershipAsync(callerId, isAdmin, hotelId, roomTypeId);

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.RoomTypeId == roomTypeId)
                ?? throw new RoomNotFoundException(roomId);

            room.RoomNumber = request.RoomNumber;
            room.Floor = request.Floor;
            room.Notes = request.Notes;
            room.Status = Enum.Parse<RoomStatus>(request.Status);

            await _context.SaveChangesAsync();
            return MapToDto(room);
        }
        public async Task<List<RoomAvailabilityDto>> GetAvailabilityAsync(long roomId, DateOnly from, DateOnly to)
        {
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId);
            if (!roomExists)
                throw new RoomNotFoundException(roomId);

            var existingRecords = await _context.RoomAvailabilities
                .Where(a => a.RoomId == roomId && a.Date >= from && a.Date <= to)
                .ToDictionaryAsync(a => a.Date);

            var result = new List<RoomAvailabilityDto>();
            var current = from;

            while (current <= to)
            {
                if (existingRecords.TryGetValue(current, out var record))
                    result.Add(new RoomAvailabilityDto(record.Date, record.Status.ToString(), record.PriceOverride));
                else
                    result.Add(new RoomAvailabilityDto(current, RoomAvailabilityStatus.Free.ToString(), null));

                current = current.AddDays(1);
            }

            return result;
        }
        public async Task SetAvailabilityAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId, long roomId, SetAvailabilityRequest request)
        {
            await VerifyOwnershipAsync(callerId, isAdmin, hotelId, roomTypeId);
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.RoomTypeId == roomTypeId)
                ?? throw new RoomNotFoundException(roomId);
            var status = Enum.Parse<RoomAvailabilityStatus>(request.Status);

            var existingRecords = await _context.RoomAvailabilities
                .Where(ra => ra.RoomId == roomId && ra.Date >= request.From && ra.Date <= request.To)
                .ToListAsync();

            var existingDates = existingRecords.ToDictionary(a => a.Date);
            var current = request.From;
            while (current <= request.To)
            {
                if (existingDates.TryGetValue(current, out var existing))
                {
                    existing.Status = status;
                    existing.PriceOverride = request.PriceOverride;
                }
                else
                {
                    _context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomId = roomId,
                        Date = current,
                        Status = status,
                        PriceOverride = request.PriceOverride
                    });
                }

                current = current.AddDays(1);
            }
            await _context.SaveChangesAsync();

        }
        private async Task VerifyOwnershipAsync(long callerId, bool isAdmin, long hotelId, long roomTypeId)
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == hotelId)
                ?? throw new HotelNotFoundException(hotelId);

            if (hotel.OwnerId != callerId && !isAdmin)
                throw new UnAuthoraizedOwnerException();

            var roomTypeExists = await _context.RoomTypes
                .AnyAsync(rt => rt.Id == roomTypeId && rt.HotelId == hotelId);
            if (!roomTypeExists)
                throw new RoomTypeNotFoundException(roomTypeId);
        }
        private static RoomDto MapToDto(Room r) => new(
            r.Id, r.RoomTypeId, r.RoomNumber, r.Floor, r.Notes, r.Status.ToString()
        );

    }
}
