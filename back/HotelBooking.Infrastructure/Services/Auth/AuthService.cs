using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelBooking.Application.DTOs.Auth;
using HotelBooking.Application.Interfaces;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enum;
using HotelBooking.Domain.Exceptions;
using HotelBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(UserManager<User> userManager, AppDbContext context, IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = _userManager.FindByEmailAsync(request.Email).Result;
            if (existingUser is not null)
                throw new ArgumentException("User with this email already exists.");

            var role = request.Role.ToLower() switch
            {
                "owner" => UserRole.Owner,
                "guest" => UserRole.Guest,
                _ => UserRole.Guest
            };

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                Role = role
            };

            var result = _userManager.CreateAsync(user, request.Password).Result;
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            return await GenerateAuthResponseAsync(user);

        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new AuthResponse(
                UserId: user.Id,
                Username: user.UserName,
                Email: user.Email!,
                Role: user.Role.ToString(),
                AccessToken: accessToken.token,
                RefreshToken: refreshToken,
                AccessTokenExpiry: accessToken.expiry
            );

        }

        private async Task<string> CreateRefreshTokenAsync(long userId)
        {
            var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = tokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(
                    double.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
            };

            _context.Add(refreshToken);
            await _context.SaveChangesAsync();
            return tokenValue;
        }

        private (string token, DateTime expiry) GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenExpiryMinutes"]!, CultureInfo.InvariantCulture));

            var Claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("username", user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: Claims,
                expires: expiry,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Invalid email or password.");

            // CHANGED BY AI (2026-07-13): please review. This previously never checked Identity's
            // built-in lockout fields (LockoutEnd/LockoutEnabled), so suspending an account via
            // those fields had no actual effect on login. Now enforced, backing the admin
            // suspend/unsuspend feature (see UsersController).
            if (await _userManager.IsLockedOutAsync(user))
                throw new UserSuspendedException(user.LockoutEnd);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token is null || !token.IsActive)
                throw new Exception("Invalid refresh token.");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(token.User);
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.
                FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token is null || !token.IsActive)
                throw new Exception("Invalid refresh token.");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

    }
}
