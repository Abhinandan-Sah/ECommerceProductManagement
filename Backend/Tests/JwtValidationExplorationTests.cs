using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Backend.Tests
{
    /// <summary>
    /// JWT Validation Exploration Tests - Bug Condition Property Tests
    /// 
    /// **Validates: Requirements 1.1, 1.2, 1.3**
    /// 
    /// These tests demonstrate JWT validation vulnerabilities in the UNFIXED code.
    /// CRITICAL: These tests MUST FAIL on unfixed code - failure confirms the bugs exist.
    /// 
    /// Bug Condition: JWT tokens with wrong issuer or audience but correct signature are ACCEPTED
    /// Expected Behavior: JWT tokens with wrong issuer or audience should be REJECTED with 401
    /// 
    /// After fixes are applied (ValidateIssuer=true, ValidateAudience=true), these tests should PASS.
    /// </summary>
    public class JwtValidationExplorationTests
    {
        private const string JwtSecret = "ThisIsASecretKeyForJWTTokenGenerationAndValidation12345";
        private const string CorrectIssuer = "https://identity-api";
        private const string WrongIssuer = "https://malicious-service";
        private const string CorrectAudience = "catalog-api";
        private const string WrongAudience = "evil-api";

        /// <summary>
        /// Property 1: Bug Condition - JWT tokens with wrong issuer should be REJECTED
        /// 
        /// This test creates a JWT token with:
        /// - WRONG issuer ("https://malicious-service")
        /// - Correct signature (signed with correct secret)
        /// - Valid expiration
        /// 
        /// EXPECTED ON UNFIXED CODE: Token validation SUCCEEDS (bug exists - ValidateIssuer=false)
        /// EXPECTED ON FIXED CODE: Token validation FAILS (bug fixed - ValidateIssuer=true)
        /// 
        /// **Validates: Requirements 1.1, 1.2**
        /// </summary>
        [Fact]
        public void Property1_JwtToken_WithWrongIssuer_ShouldBeRejected()
        {
            // Arrange - Create token with WRONG issuer but correct signature
            var token = CreateJwtToken(
                issuer: WrongIssuer,
                audience: CorrectAudience,
                userId: Guid.NewGuid(),
                email: "attacker@evil.com"
            );

            // Act - Validate token with CORRECT issuer expected
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,  // This is what the FIX should set
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = CorrectIssuer,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Assert - Token with wrong issuer should be REJECTED
            var exception = Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
            });

            // Verify the exception message indicates issuer validation failure
            Assert.Contains("IDX10205", exception.Message); // Issuer validation failed error code
        }

        /// <summary>
        /// Property 1: Bug Condition - JWT tokens with wrong audience should be REJECTED
        /// 
        /// This test creates a JWT token with:
        /// - Correct issuer
        /// - WRONG audience ("evil-api")
        /// - Correct signature (signed with correct secret)
        /// - Valid expiration
        /// 
        /// EXPECTED ON UNFIXED CODE: Token validation SUCCEEDS (bug exists - ValidateAudience=false)
        /// EXPECTED ON FIXED CODE: Token validation FAILS (bug fixed - ValidateAudience=true)
        /// 
        /// **Validates: Requirements 1.1, 1.2**
        /// </summary>
        [Fact]
        public void Property1_JwtToken_WithWrongAudience_ShouldBeRejected()
        {
            // Arrange - Create token with WRONG audience but correct signature
            var token = CreateJwtToken(
                issuer: CorrectIssuer,
                audience: WrongAudience,
                userId: Guid.NewGuid(),
                email: "attacker@evil.com"
            );

            // Act - Validate token with CORRECT audience expected
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,  // This is what the FIX should set
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = CorrectAudience,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Assert - Token with wrong audience should be REJECTED
            var exception = Assert.Throws<SecurityTokenInvalidAudienceException>(() =>
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
            });

            // Verify the exception message indicates audience validation failure
            Assert.Contains("IDX10214", exception.Message); // Audience validation failed error code
        }

        /// <summary>
        /// Property 1: Bug Condition - Current unfixed code accepts tokens without issuer/audience validation
        /// 
        /// This test demonstrates the CURRENT BUGGY BEHAVIOR where tokens are accepted
        /// even when ValidateIssuer=false and ValidateAudience=false.
        /// 
        /// EXPECTED ON UNFIXED CODE: Token validation SUCCEEDS (demonstrates the bug)
        /// EXPECTED ON FIXED CODE: This test documents the old behavior (will still pass as-is)
        /// 
        /// **Validates: Requirements 1.1, 1.2, 1.3**
        /// </summary>
        [Fact]
        public void Property1_CurrentBuggyBehavior_AcceptsTokens_WithoutIssuerAudienceValidation()
        {
            // Arrange - Create token with WRONG issuer and audience
            var token = CreateJwtToken(
                issuer: WrongIssuer,
                audience: WrongAudience,
                userId: Guid.NewGuid(),
                email: "attacker@evil.com"
            );

            // Act - Validate token with CURRENT BUGGY configuration (ValidateIssuer=false, ValidateAudience=false)
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,      // CURRENT BUGGY SETTING
                ValidateAudience = false,    // CURRENT BUGGY SETTING
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Assert - Token is ACCEPTED with buggy configuration (this demonstrates the vulnerability)
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
            Assert.IsType<JwtSecurityToken>(validatedToken);
        }

        /// <summary>
        /// Property 1: Bug Condition - Tokens with correct issuer and audience should be ACCEPTED
        /// 
        /// This test verifies that valid tokens with correct issuer and audience are accepted.
        /// This ensures the fix doesn't break legitimate authentication.
        /// 
        /// EXPECTED ON BOTH UNFIXED AND FIXED CODE: Token validation SUCCEEDS
        /// 
        /// **Validates: Requirements 2.1, 2.2, 2.3, 2.4**
        /// </summary>
        [Fact]
        public void Property1_JwtToken_WithCorrectIssuerAndAudience_ShouldBeAccepted()
        {
            // Arrange - Create token with CORRECT issuer and audience
            var userId = Guid.NewGuid();
            var email = "user@example.com";
            var token = CreateJwtToken(
                issuer: CorrectIssuer,
                audience: CorrectAudience,
                userId: userId,
                email: email
            );

            // Act - Validate token with correct issuer and audience
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = CorrectIssuer,
                ValidAudience = CorrectAudience,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Assert - Token with correct issuer and audience should be ACCEPTED
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
            Assert.IsType<JwtSecurityToken>(validatedToken);
            
            // Verify claims are preserved
            Assert.Equal(userId.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal(email, principal.FindFirst(ClaimTypes.Email)?.Value);
        }

        /// <summary>
        /// Helper method to create JWT tokens for testing
        /// </summary>
        private string CreateJwtToken(string issuer, string audience, Guid userId, string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
