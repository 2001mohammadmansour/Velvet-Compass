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
        // Section slugs the public page knows how to render. An unknown/blank value falls back
        // to "Other" so a bad payload can never hide a partner from every section.
        private static readonly string[] KnownCategories =
            { "CarRental", "Dining", "Tours", "Transport", "Other" };

        private readonly AppDbContext _context;
        public PartnerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartnerDto>> GetAllAsync()
        {
            var partners = await _context.Partners
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            return partners.Select(MapToDto).ToList();
        }

        public async Task<PartnerDto> CreateAsync(CreatePartnerRequest request)
        {
            var partner = new Partner
            {
                Name = request.Name,
                Cities = NormalizeCities(request.Cities),
                Description = request.Description,
                Category = NormalizeCategory(request.Category),
                WebsiteUrl = NormalizeUrl(request.WebsiteUrl),
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
            partner.Cities = NormalizeCities(request.Cities);
            partner.Description = request.Description;
            partner.Category = NormalizeCategory(request.Category);
            partner.WebsiteUrl = NormalizeUrl(request.WebsiteUrl);

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

        public async Task RegisterClickAsync(long partnerId)
        {
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId)
                ?? throw new PartnerNotFoundException(partnerId);

            partner.ClickCount++;
            await _context.SaveChangesAsync();
        }

        // Trim, drop blanks, de-duplicate (case-insensitive) while keeping the admin's order.
        private static List<string> NormalizeCities(IEnumerable<string>? cities)
        {
            var result = new List<string>();
            foreach (var raw in cities ?? Enumerable.Empty<string>())
            {
                var city = (raw ?? "").Trim();
                if (city.Length == 0) continue;
                if (!result.Any(c => string.Equals(c, city, StringComparison.OrdinalIgnoreCase)))
                    result.Add(city);
            }
            if (result.Count == 0)
                throw new InvalidRequestException("A partner must be listed in at least one city.");
            return result;
        }

        private static string NormalizeCategory(string? category)
        {
            var trimmed = (category ?? "").Trim();
            return KnownCategories.FirstOrDefault(c => string.Equals(c, trimmed, StringComparison.OrdinalIgnoreCase))
                   ?? "Other";
        }

        private static string? NormalizeUrl(string? url)
        {
            var trimmed = (url ?? "").Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static PartnerDto MapToDto(Partner p) => new(
            p.Id, p.Name, p.Cities, p.Description, p.ImageUrl, p.Category, p.WebsiteUrl, p.ClickCount
        );
    }
}
