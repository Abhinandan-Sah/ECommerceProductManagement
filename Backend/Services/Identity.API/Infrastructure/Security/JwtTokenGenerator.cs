using Identity.API.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Identity.API.Infrastructure.Security
{
    public class JwtTokenGenerator
    {
        private readonly string _secret;
        private readonly int _expiryMinutes;

        public JwtTokenGenerator(IConfiguration config)
        {
            _secret = config["JwtSettings:Secret"]
                        ?? throw new Exception("JWT Secret not configured");
            _expiryMinutes = config.GetValue<int>("JwtSettings:ExpiryMinutes", 60);
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetTokenExpiry() => DateTime.UtcNow.AddMinutes(_expiryMinutes);

        /// <summary>
        /// Generates a cryptographically secure random refresh token string.
        /// </summary>
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}

