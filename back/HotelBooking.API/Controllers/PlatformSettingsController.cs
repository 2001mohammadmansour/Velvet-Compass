using HotelBooking.Application.DTOs.Platform;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/platform-settings")]
    public class PlatformSettingsController : ControllerBase
    {
        private readonly IPlatformSettingsService _service;
        private readonly IFileStorageService _fileStorageService;

        public PlatformSettingsController(IPlatformSettingsService service, IFileStorageService fileStorageService)
        {
            _service = service;
            _fileStorageService = fileStorageService;
        }

        // Owner + Admin: owners need the platform wallet to pay their commission.
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Get() => Ok(await _service.GetAsync());

        [HttpPut("shamcash")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateShamCash([FromBody] UpdatePlatformShamCashRequest request)
            => Ok(await _service.UpdateShamCashWalletAsync(request.ShamCashWallet));

        [HttpPost("shamcash-qr/upload")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadShamCashQr(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidFileUploadException("No file was uploaded.");

            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, "shamcash");
            return Ok(await _service.SetShamCashQrAsync(url));
        }
    }
}
