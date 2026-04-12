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
        private readonly string _issuer;
        private readonly string[] _audiences;

        public JwtTokenGenerator(IConfiguration config)
        {
            _secret = config["JwtSettings:Secret"]
                        ?? throw new Exception("JWT Secret not configured");
            _expiryMinutes = config.GetValue<int>("JwtSettings:ExpiryMinutes", 60);
            _issuer = config["JwtSettings:Issuer"]
                        ?? throw new Exception("JWT Issuer not configured");
            
            // Support both single audience and multiple audiences
            var singleAudience = config["JwtSettings:Audience"];
            var multipleAudiences = config.GetSection("JwtSettings:Audiences").Get<string[]>();
            
            if (multipleAudiences != null && multipleAudiences.Length > 0)
            {
                _audiences = multipleAudiences;
            }
            else if (!string.IsNullOrEmpty(singleAudience))
            {
                _audiences = new[] { singleAudience };
            }
            else
            {
                throw new Exception("JWT Audience(s) not configured");
            }
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
                issuer: _issuer,
                audience: _audiences[0], // JWT spec requires single audience in standard claim, but we can add multiple
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

