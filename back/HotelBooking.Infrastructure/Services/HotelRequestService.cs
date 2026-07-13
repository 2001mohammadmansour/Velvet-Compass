using HotelBooking.Application.DTOs.HotelRequests;
using HotelBooking.Application.DTOs.Hotels;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class HotelRequestService : IHotelRequestService
    {
        private readonly AppDbContext _context;
        private readonly IHotelService _hotelService;
        private readonly INotificationService _notificationService;

        public HotelRequestService(AppDbContext context, IHotelService hotelService, INotificationService notificationService)
        {
            _context = context;
            _hotelService = hotelService;
            _notificationService = notificationService;
        }

        public async Task<HotelRequestDto> SubmitAsync(long ownerId, SubmitHotelRequestRequest request)
        {
            var type = string.Equals(request.Type, "edit", StringComparison.OrdinalIgnoreCase)
                ? HotelRequestType.Edit
                : HotelRequestType.Create;

            var hotelRequest = new HotelRequest
            {
                OwnerId = ownerId,
                Type = type,
                HotelId = request.HotelId,
                Status = HotelRequestStatus.Pending,
                HotelName = request.HotelName,
                City = request.City,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Description = request.Description,
                Stars = request.Stars,
                DocumentDataUrl = request.DocumentDataUrl
            };

            _context.HotelRequests.Add(hotelRequest);
            await _context.SaveChangesAsync();
            return await MapToDtoAsync(hotelRequest);
        }

        public async Task<List<HotelRequestDto>> GetMyRequestsAsync(long ownerId)
        {
            var requests = await _context.HotelRequests
                .Where(r => r.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return await MapToDtoListAsync(requests);
        }

        public async Task<List<HotelRequestDto>> GetAllAsync()
        {
            var requests = await _context.HotelRequests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return await MapToDtoListAsync(requests);
        }

        public async Task<HotelRequestDto> ApproveAsync(long requestId)
        {
            var req = await _context.HotelRequests.FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new HotelRequestNotFoundException(requestId);

            if (req.Status != HotelRequestStatus.Pending)
                throw new InvalidAdminActionException("Only pending requests can be approved.");

            var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.OwnerId)
                ?? throw new UserNotFoundException(req.OwnerId);

            if (req.Type == HotelRequestType.Create)
            {
                var created = await _hotelService.CreateHotelAsync(new CreateHotelRequest(
                    owner.Email!,
                    req.HotelName ?? "",
                    req.Description ?? "",
                    req.Address ?? "",
                    req.City ?? "",
                    "Syria",
                    req.Stars ?? 3,
                    req.PhoneNumber ?? "",
                    owner.Email!
                ));
                req.HotelId = created.HotelId;
            }
            else
            {
                if (!req.HotelId.HasValue)
                    throw new InvalidAdminActionException("This edit request has no target hotel.");

                var current = await _hotelService.GetByIdAsync(req.HotelId.Value);
                // Only the fields the request actually changed are overridden; everything else
                // carries through unchanged, same merge pattern used elsewhere in this app.
                await _hotelService.UpdateHotelAsync(owner.Id, true, req.HotelId.Value, new UpdateHotelRequest(
                    req.HotelName ?? current.Name,
                    req.Description ?? current.Description,
                    req.Address ?? current.Address,
                    req.City ?? current.City,
                    current.Country,
                    req.Stars ?? current.StarRating,
                    req.PhoneNumber ?? current.Phone,
                    current.Email
                ));
            }

            req.Status = HotelRequestStatus.Approved;
            req.ReviewedAt = DateTime.UtcNow;
            req.RejectionReason = null;
            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature.
            await _notificationService.CreateAsync(
                req.OwnerId, NotificationType.HotelRequestApproved.ToString(),
                "Hotel request approved",
                $"Your {req.Type.ToString().ToLower()} request for \"{req.HotelName ?? "your hotel"}\" was approved.",
                relatedHotelRequestId: req.Id);

            return await MapToDtoAsync(req);
        }

        public async Task<HotelRequestDto> RejectAsync(long requestId, string reason)
        {
            var req = await _context.HotelRequests.FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new HotelRequestNotFoundException(requestId);

            if (req.Status != HotelRequestStatus.Pending)
                throw new InvalidAdminActionException("Only pending requests can be rejected.");

            req.Status = HotelRequestStatus.Rejected;
            req.ReviewedAt = DateTime.UtcNow;
            req.RejectionReason = reason;
            await _context.SaveChangesAsync();

            // CHANGED BY AI (2026-07-13): please review. New notification for the Notifications
            // feature.
            await _notificationService.CreateAsync(
                req.OwnerId, NotificationType.HotelRequestRejected.ToString(),
                "Hotel request rejected",
                $"Your {req.Type.ToString().ToLower()} request for \"{req.HotelName ?? "your hotel"}\" was rejected. Reason: {reason}",
                relatedHotelRequestId: req.Id);

            return await MapToDtoAsync(req);
        }

        private async Task<HotelRequestDto> MapToDtoAsync(HotelRequest r)
        {
            var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == r.OwnerId);
            return MapToDto(r, owner);
        }

        private async Task<List<HotelRequestDto>> MapToDtoListAsync(List<HotelRequest> requests)
        {
            var ownerIds = requests.Select(r => r.OwnerId).Distinct().ToList();
            var owners = await _context.Users.Where(u => ownerIds.Contains(u.Id)).ToListAsync();
            var ownersById = owners.ToDictionary(u => u.Id);
            return requests
                .Select(r => MapToDto(r, ownersById.TryGetValue(r.OwnerId, out var o) ? o : null))
                .ToList();
        }

        private static HotelRequestDto MapToDto(HotelRequest r, User? owner) => new(
            r.Id,
            r.Type.ToString(),
            r.Status.ToString(),
            r.OwnerId,
            owner?.UserName ?? "",
            owner?.Email ?? "",
            r.HotelId,
            r.HotelName,
            r.City,
            r.Address,
            r.PhoneNumber,
            r.Description,
            r.Stars,
            r.DocumentDataUrl,
            r.RejectionReason,
            r.CreatedAt,
            r.ReviewedAt
        );
    }
}
