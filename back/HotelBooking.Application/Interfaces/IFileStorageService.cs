namespace HotelBooking.Application.Interfaces
{
    // CHANGED BY AI (2026-07-13): please review. New abstraction backing real hotel/room-type
    // photo uploads (previously the frontend called a mock "/api/uploads/signed-urls" endpoint
    // that doesn't exist on this backend at all). Takes a raw stream rather than IFormFile so this
    // interface has no dependency on ASP.NET Core hosting types.
    public interface IFileStorageService
    {
        // subFolder groups files on disk (e.g. "hotels", "room-types"). Returns the absolute,
        // publicly-reachable URL to store on the HotelImage/RoomTypeImage record.
        Task<string> SaveImageAsync(Stream content, string originalFileName, string contentType, string subFolder);
    }
}
