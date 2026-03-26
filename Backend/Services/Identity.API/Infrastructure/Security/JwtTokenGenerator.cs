using Identity.API.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Infrastructure.Security
{
    public class JwtTokenGenerator
    {
        private readonly string _secret;

        public JwtTokenGenerator(IConfiguration _config)
        {
            _secret = _config["JwtSettings:Secret"]
                        ?? throw new Exception("JWT Secret not configured");
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 2. Create Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

            // 3. Create Credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Create Token
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24*7),
                signingCredentials: creds
            );

            // 5. Return Token String
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
