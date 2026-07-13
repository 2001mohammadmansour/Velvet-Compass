using HotelBooking.Domain.Enum;

namespace HotelBooking.Domain.Entities
{
    // A hotel owner's request to either create a new hotel or edit their existing one.
    // Requires admin approval before it takes effect (see HotelRequestService).
    public class HotelRequest : BaseEntity
    {
        public long OwnerId { get; set; }
        public HotelRequestType Type { get; set; }
        public long? HotelId { get; set; } // only set for Edit requests
        public HotelRequestStatus Status { get; set; } = HotelRequestStatus.Pending;

        // Requested field values — only the ones being changed need to be populated.
        public string? HotelName { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public short? Stars { get; set; }

        // Stored as a base64 data URL directly, since there's no file storage backend yet.
        public string? DocumentDataUrl { get; set; }

        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public User Owner { get; set; } = null!;
        public Hotel? Hotel { get; set; }
    }
}
