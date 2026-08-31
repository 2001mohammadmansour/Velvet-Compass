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
using QRCoder;

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
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                throw new InvalidRequestException("User with this email already exists.");

            // UserName is a real, independently-editable username (backs the Edit Profile
            // feature). Falls back to the email if left blank so registration can't fail on
            // this alone.
            var desiredUsername = string.IsNullOrWhiteSpace(request.Username) ? request.Email : request.Username.Trim();
            var existingUsername = await _userManager.FindByNameAsync(desiredUsername);
            if (existingUsername is not null)
                throw new InvalidRequestException("That username is already taken.");

            var role = request.Role.ToLower() switch
            {
                "owner" => UserRole.Owner,
                "guest" => UserRole.Guest,
                _ => UserRole.Guest
            };

            var user = new User
            {
                UserName = desiredUsername,
                Email = request.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Role = role
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
            return await GenerateAuthResponseAsync(user);

        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new InvalidCredentialsException("Invalid email or password.");

            // Enforces Identity's built-in lockout fields (LockoutEnd/LockoutEnabled), which
            // back the admin suspend/unsuspend feature (see UserService).
            if (await _userManager.IsLockedOutAsync(user))
                throw new UserSuspendedException(user.LockoutEnd);

            if (!user.TwoFactorEnabled)
            {
                var auth = await GenerateAuthResponseAsync(user);
                return new LoginResult(false, null, auth);
            }

            // ─── 2FA مفعّل: Challenge مؤقت بدل توكنات مباشرة ──────
            var challengeToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            _context.TwoFactorChallenges.Add(new TwoFactorChallenge
            {
                UserId = user.Id,
                ChallengeToken = challengeToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            });
            await _context.SaveChangesAsync();

            return new LoginResult(true, challengeToken, null);
            //return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token is null || !token.IsActive)
                throw new InvalidRequestException("Invalid refresh token.");

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
                throw new InvalidRequestException("Invalid refresh token.");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // 2FA
        public async Task<Setup2FAResponse> SetupTwoFactorAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var qrCodeImage = GenerateQrCodeBase64(user.Email!, key!);
            return new Setup2FAResponse(FormatKey(key!), qrCodeImage);
        }

        public async Task<Enable2FAResponse> EnableTwoFactorAsync(long userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultAuthenticatorProvider, code);

            if (!isValid)
                throw new InvalidRequestException("Invalid verification code.");

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return new Enable2FAResponse(recoveryCodes!.ToList());
        }

        public async Task DisableTwoFactorAsync(long userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            if (!await _userManager.CheckPasswordAsync(user, password))
                throw new InvalidCredentialsException("Incorrect password.");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
        }

        public async Task<AuthResponse> VerifyTwoFactorAsync(Verify2FARequest request)
        {
            var challenge = await _context.TwoFactorChallenges
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ChallengeToken == request.ChallengeToken)
                ?? throw new InvalidRequestException("Invalid verification session.");

            if (!challenge.IsValid)
                throw new InvalidRequestException("Verification session expired or invalid, please log in again.");

            if (await _userManager.IsLockedOutAsync(challenge.User))
                throw new UserSuspendedException(challenge.User.LockoutEnd);

            bool isValid;

            if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
            {
                var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(challenge.User, request.RecoveryCode);
                isValid = result.Succeeded;
            }
            else if (!string.IsNullOrWhiteSpace(request.Code))
            {
                isValid = await _userManager.VerifyTwoFactorTokenAsync(
                    challenge.User, TokenOptions.DefaultAuthenticatorProvider, request.Code);
            }
            else
            {
                throw new InvalidRequestException("You must send either a code or a recoveryCode.");
            }

            if (!isValid)
            {
                challenge.FailedAttempts++;
                await _context.SaveChangesAsync();
                throw new InvalidRequestException("Invalid code.");
            }

            challenge.IsUsed = true;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(challenge.User);
        }

        public async Task<List<string>> RegenerateRecoveryCodesAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException(userId);

            if (!user.TwoFactorEnabled)
                throw new InvalidRequestException("Two-factor authentication is not enabled on this account.");

            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            return codes!.ToList();
        }

        // Helper
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
        private static string GenerateQrCodeBase64(string email, string unformattedKey)
        {
            var issuer = Uri.EscapeDataString("HotelBooking");
            var label = Uri.EscapeDataString(email);
            var uri = $"otpauth://totp/{issuer}:{label}?secret={unformattedKey}&issuer={issuer}&digits=6";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var bytes = qrCode.GetGraphic(20);

            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            for (int i = 0; i < unformattedKey.Length; i += 4)
            {
                result.Append(unformattedKey.AsSpan(i, Math.Min(4, unformattedKey.Length - i)));
                result.Append(' ');
            }
            return result.ToString().TrimEnd();
        }

    }
}
