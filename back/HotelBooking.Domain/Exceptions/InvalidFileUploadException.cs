namespace HotelBooking.Domain.Exceptions
{
    // CHANGED BY AI (2026-07-13): please review. New exception for the hotel/room-type photo
    // upload endpoints — thrown for missing files, disallowed extensions, or oversized uploads.
    public class InvalidFileUploadException : Exception
    {
        public InvalidFileUploadException(string message) : base(message) { }
    }
}
