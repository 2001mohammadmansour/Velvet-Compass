using System.Net;
using System.Text.Json;
using HotelBooking.Domain.Exceptions;

namespace HotelBooking.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {
            var (statusCode, message) = ex switch
            {
                HotelNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                RoomTypeNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                RoomNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                BookingNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                RoomNotAvailableException => (HttpStatusCode.Conflict, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mapping for the room-type-has-
                // bookings delete guard.
                RoomTypeHasBookingsException => (HttpStatusCode.Conflict, ex.Message),
                UnAuthoraizedOwnerException => (HttpStatusCode.Forbidden, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mappings for user suspension
                // (login-time) and admin user-management guard rails.
                UserSuspendedException => (HttpStatusCode.Forbidden, ex.Message),
                UserNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                InvalidAdminActionException => (HttpStatusCode.BadRequest, ex.Message),
                HotelRequestNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mapping for the photo upload
                // endpoints (bad extension, oversized file, missing file).
                InvalidFileUploadException => (HttpStatusCode.BadRequest, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mappings for the Reviews feature.
                AlreadyReviewedException => (HttpStatusCode.Conflict, ex.Message),
                ReviewNotEligibleException => (HttpStatusCode.BadRequest, ex.Message),
                ReviewNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mapping for the modify-booking-
                // dates guard rails (cancelled/completed/already-started bookings).
                BookingNotModifiableException => (HttpStatusCode.BadRequest, ex.Message),
                // CHANGED BY AI (2026-07-13): please review. New mapping for the Trips feature.
                PartnerNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                // CHANGED BY AI (2026-07-15): please review. New mappings for the amenities catalog
                // and the extra-bed system (bad config, guest count over effective capacity).
                AmenityNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                GuestCountExceedsCapacityException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidRoomTypeConfigurationException => (HttpStatusCode.BadRequest, ex.Message),
                // CHANGED BY AI (2026-07-15): please review. New mappings for the seasonal-rule /
                // occupancy-tier pricing system.
                PricingRuleNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                OverlappingPriceRuleException => (HttpStatusCode.Conflict, ex.Message),
                InvalidPricingRuleException => (HttpStatusCode.BadRequest, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            // Only the generic 500 branch is a real bug/misconfiguration worth surfacing — the
            // named domain exceptions above are expected control flow (404s, validation, etc.).
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(response);

        }
    }
}
