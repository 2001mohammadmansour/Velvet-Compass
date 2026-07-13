using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Exceptions;

namespace HotelBooking.API.Services
{
    // CHANGED BY AI (2026-07-13): please review. Local-disk implementation of IFileStorageService
    // — saves under wwwroot/uploads/{subFolder} and returns an absolute URL built from Uploads:BaseUrl
    // (appsettings.json). Lives in the API project (rather than Infrastructure) because it needs
    // IWebHostEnvironment, which is only naturally available in a Web-SDK project.
    public class FileStorageService : IFileStorageService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly IWebHostEnvironment _env;
        private readonly string _baseUrl;

        public FileStorageService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _baseUrl = configuration["Uploads:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5001";
        }

        public async Task<string> SaveImageAsync(Stream content, string originalFileName, string contentType, string subFolder)
        {
            if (content.Length == 0)
                throw new InvalidFileUploadException("The uploaded file is empty.");
            if (content.Length > MaxFileSizeBytes)
                throw new InvalidFileUploadException("Images must be 5 MB or smaller.");

            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new InvalidFileUploadException("Only .jpg, .jpeg, .png, and .webp images are allowed.");

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(webRootPath, "uploads", subFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await content.CopyToAsync(fileStream);
            }

            return $"{_baseUrl}/uploads/{subFolder}/{fileName}";
        }
    }
}
