using HotelBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.API.Controllers
{
    // CHANGED BY AI (2026-07-13): please review. New public endpoint backing the homepage's
    // platform-stats section (previously called a mock "/api/stats" endpoint that never existed
    // on this backend — the numbers always silently failed and showed "—"). Injects AppDbContext
    // directly rather than adding a whole new service layer for four simple counts, matching the
    // pattern already used in UsersController.
    [ApiController]
    [Route("api/v1/stats")]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var hotels = await _context.Hotels.CountAsync();
            var cities = await _context.Hotels.Select(h => h.City).Distinct().CountAsync();
            var bookings = await _context.Bookings.CountAsync();
            var rooms = await _context.Rooms.CountAsync();

            return Ok(new { hotels, cities, bookings, rooms });
        }
    }
}
