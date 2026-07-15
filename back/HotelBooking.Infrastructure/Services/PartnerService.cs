using HotelBooking.Application.DTOs.Partners;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly AppDbContext _context;
        public PartnerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartnerDto>> GetAllAsync()
        {
            return await _context.Partners
                .OrderBy(p => p.CreatedAt)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PartnerDto> CreateAsync(CreatePartnerRequest request)
        {
            var partner = new Partner
            {
                Name = request.Name,
                City = request.City,
                Description = request.Description,
            };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            return MapToDto(partner);
        }

        public async Task<PartnerDto> UpdateAsync(long partnerId, UpdatePartnerRequest request)
        {
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId)
                ?? throw new PartnerNotFoundException(partnerId);

            partner.Name = request.Name;
            partner.City = request.City;
            partner.Description = request.Description;

            await _context.SaveChangesAsync();
            return MapToDto(partner);
        }

        public async Task DeleteAsync(long partnerId)
        {
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId)
                ?? throw new PartnerNotFoundException(partnerId);

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();
        }

        public async Task<PartnerDto> SetImageAsync(long partnerId, string imageUrl)
        {
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId)
                ?? throw new PartnerNotFoundException(partnerId);

            partner.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();
            return MapToDto(partner);
        }

        private static PartnerDto MapToDto(Partner p) => new(
            p.Id, p.Name, p.City, p.Description, p.ImageUrl
        );
    }
}
