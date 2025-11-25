
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.Services;
using ParcelwalaAPP.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Parcelwala.DataAccess.Services
{

    public interface IJwtService
    {
        // string GenerateTokenForUser(Users user);
       string GenerateAccessToken(Users user);
        string GenerateRefreshToken();
        Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token, string? ipAddress);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress, string? replacedByToken = null);
        Task RemoveExpiredRefreshTokensAsync(int userId);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OtpAuthService> _logger;
        private readonly AppDbContext _context;
        public JwtService(IConfiguration configuration, ILogger<OtpAuthService> logger,AppDbContext context)
        {
            _configuration = configuration;
            _logger= logger;    
            _context= context;

        }


        public string GenerateAccessToken(Users user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ParcelwalaAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "ParcelwalaClients";
            var accessTokenLifetimeMinutes =Convert.ToInt32( _configuration["Jwt:accessTokenLifetimeMinutes"]) ;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtId = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim("phone", user.PhoneNumber),
                new Claim("fullName", user.FullName),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenLifetimeMinutes), // Short-lived: 15 minutes
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Access token generated for Customer ID: {CustomerId}, JTI: {JwtId}",
                user.UserID, jwtId);

            return accessToken;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token, string? ipAddress)
        {
            var RefreshTokenLifetimeDays =Convert.ToInt32( _configuration["Jwt:RefreshTokenLifetimeDays"]);
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays), // 
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Clean up expired tokens for this user
            await RemoveExpiredRefreshTokensAsync(userId);

            return refreshToken;
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string? ipAddress, string? replacedByToken = null)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = replacedByToken;

            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task RemoveExpiredRefreshTokensAsync(int userId)
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                            (rt.IsRevoked || rt.ExpiresAt <= DateTime.UtcNow))
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }

        public ClaimsPrincipal? ValidateExpiredToken(string token)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key not configured");

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Don't check expiration for refresh
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token algorithm");
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating expired token");
                return null;
            }
        }

        public string? GetJwtIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting JWT ID from token");
                return null;
            }
        }

        //public string GenerateTokenForUser(Users user)
        //{
        //    var securityKey = new SymmetricSecurityKey(
        //        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        //    var credentials = new SigningCredentials(
        //        securityKey,
        //        SecurityAlgorithms.HmacSha256);

        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
        //        new Claim("userId", user.UserID.ToString()),
        //        new Claim("phoneNumber", user.PhoneNumber),
        //        new Claim("isVerified", user.IsVerified.ToString()),
        //        new Claim("phoneVerified", user.PhoneVerified.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    // Add optional claims
        //    if (!string.IsNullOrWhiteSpace(user.UserType))
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, user.UserType));
        //        claims.Add(new Claim("userType", user.UserType));
        //    }

        //    if (!string.IsNullOrWhiteSpace(user.Email))
        //    {
        //        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        //        claims.Add(new Claim(ClaimTypes.Email, user.Email));
        //    }

        //    if (!string.IsNullOrWhiteSpace(user.FullName))
        //    {
        //        claims.Add(new Claim(ClaimTypes.Name, user.FullName));
        //        claims.Add(new Claim("fullName", user.FullName));
        //    }

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddHours(24), // Token valid for 24 hours
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }

}
