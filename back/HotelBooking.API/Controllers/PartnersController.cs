using HotelBooking.Application.DTOs.Partners;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly IFileStorageService _fileStorageService;

        public PartnersController(IPartnerService partnerService, IFileStorageService fileStorageService)
        {
            _partnerService = partnerService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _partnerService.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request)
            => Ok(await _partnerService.CreateAsync(request));

        [HttpPut("{partnerId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long partnerId, [FromBody] UpdatePartnerRequest request)
            => Ok(await _partnerService.UpdateAsync(partnerId, request));

        [HttpDelete("{partnerId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long partnerId)
        {
            await _partnerService.DeleteAsync(partnerId);
            return NoContent();
        }

        [HttpPost("{partnerId:long}/image/upload")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(long partnerId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidFileUploadException("No file was uploaded.");

            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, "partners");
            var result = await _partnerService.SetImageAsync(partnerId, url);
            return Ok(result);
        }
    }
}
