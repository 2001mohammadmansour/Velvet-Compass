using System.Security.Claims;

namespace HotelBooking.API.Extensions
{
    public static class ClaimsPrincipalExtentensions
    {
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub")
                ?? throw new Exception("user Id calim not found");

            return long.Parse(claim.Value);
        }
           
    }
}
