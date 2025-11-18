
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ParcelwalaAPP.Models;

namespace Parcelwala.DataAccess.Services
{

    public interface IJwtService
    {
        string GenerateTokenForUser(Users user);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateTokenForUser(Users user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim("userId", user.UserID.ToString()),
                new Claim("phoneNumber", user.PhoneNumber),
                new Claim("isVerified", user.IsVerified.ToString()),
                new Claim("phoneVerified", user.PhoneVerified.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add optional claims
            if (!string.IsNullOrWhiteSpace(user.UserType))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.UserType));
                claims.Add(new Claim("userType", user.UserType));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                claims.Add(new Claim("fullName", user.FullName));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Token valid for 24 hours
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
