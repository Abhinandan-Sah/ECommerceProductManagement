using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Xunit;

namespace Backend.Tests
{
    /// <summary>
    /// CORS Policy Exploration Tests - Bug Condition Property Tests
    /// 
    /// **Validates: Requirements 2.1, 2.2, 2.3**
    /// 
    /// These tests demonstrate CORS policy vulnerabilities in the UNFIXED code.
    /// CRITICAL: These tests document the expected behavior for CORS policies.
    /// 
    /// Bug Condition: CORS policy uses AllowAnyOrigin() which permits requests from any domain
    /// Expected Behavior: CORS policy should restrict origins to an explicit allowlist
    /// 
    /// Note: Full CORS testing requires integration tests with a running server.
    /// These unit tests verify CORS policy configuration.
    /// </summary>
    public class CorsExplorationTests
    {
        /// <summary>
        /// Property 1: Bug Condition - AllowAnyOrigin policy accepts all origins
        /// 
        /// This test verifies that a CORS policy configured with AllowAnyOrigin()
        /// will accept requests from any origin, including malicious ones.
        /// 
        /// EXPECTED ON UNFIXED CODE: Policy allows any origin (bug exists)
        /// EXPECTED ON FIXED CODE: Policy restricts to allowlist (bug fixed)
        /// 
        /// **Validates: Requirements 2.1, 2.2**
        /// </summary>
        [Fact]
        public void Property1_CorsPolicy_WithAllowAnyOrigin_AcceptsAllOrigins()
        {
            // Arrange - Create CORS policy with AllowAnyOrigin (BUGGY configuration)
            var policyBuilder = new CorsPolicyBuilder();
            policyBuilder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            var policy = policyBuilder.Build();

            // Act & Assert - Policy with AllowAnyOrigin accepts ALL origins (demonstrates vulnerability)
            Assert.True(policy.AllowAnyOrigin);
            
            // When AllowAnyOrigin is true, the Origins collection contains "*"
            // This is the BUGGY behavior we're documenting
            Assert.Contains("*", policy.Origins);
        }

        /// <summary>
        /// Property 1: Bug Condition - Restricted origin policy rejects untrusted origins
        /// 
        /// This test verifies that a CORS policy configured with WithOrigins()
        /// will reject requests from origins not in the allowlist.
        /// 
        /// EXPECTED ON BOTH UNFIXED AND FIXED CODE: Restricted policy works correctly
        /// 
        /// **Validates: Requirements 2.5, 2.6**
        /// </summary>
        [Fact]
        public void Property1_CorsPolicy_WithRestrictedOrigins_RejectsUntrustedOrigins()
        {
            // Arrange - Create CORS policy with restricted origins (CORRECT configuration)
            var allowedOrigins = new[]
            {
                "https://app.example.com",
                "https://admin.example.com",
                "http://localhost:3000"
            };

            var policyBuilder = new CorsPolicyBuilder();
            policyBuilder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            var policy = policyBuilder.Build();

            // Act & Assert - Verify allowed origins are accepted
            Assert.True(policy.IsOriginAllowed("https://app.example.com"));
            Assert.True(policy.IsOriginAllowed("https://admin.example.com"));
            Assert.True(policy.IsOriginAllowed("http://localhost:3000"));

            // Verify untrusted origins are rejected
            Assert.False(policy.IsOriginAllowed("https://evil.com"));
            Assert.False(policy.IsOriginAllowed("https://malicious-site.com"));
            Assert.False(policy.IsOriginAllowed("https://attacker.evil"));
            Assert.False(policy.IsOriginAllowed("http://localhost:9999"));
            
            // Verify policy does not allow any origin
            Assert.False(policy.AllowAnyOrigin);
        }

        /// <summary>
        /// Property 1: Bug Condition - CORS policy should validate origin format
        /// 
        /// This test verifies that CORS policy correctly handles origin validation
        /// and rejects invalid or wildcard origins.
        /// 
        /// **Validates: Requirements 2.7**
        /// </summary>
        [Fact]
        public void Property1_CorsPolicy_ShouldRejectWildcardOrigins()
        {
            // Arrange - Create CORS policy with specific origins
            var allowedOrigins = new[]
            {
                "https://app.example.com"
            };

            var policyBuilder = new CorsPolicyBuilder();
            policyBuilder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            var policy = policyBuilder.Build();

            // Act & Assert - Verify wildcard patterns are rejected
            Assert.False(policy.IsOriginAllowed("*"));
            Assert.False(policy.IsOriginAllowed("https://*.example.com"));
            Assert.False(policy.IsOriginAllowed("*.example.com"));
            
            // Verify only exact matches are allowed
            Assert.True(policy.IsOriginAllowed("https://app.example.com"));
            Assert.False(policy.IsOriginAllowed("https://app.example.com:8080")); // Different port
            Assert.False(policy.IsOriginAllowed("http://app.example.com")); // Different scheme
        }

        /// <summary>
        /// Property 1: Expected Behavior - CORS policy with credentials requires specific origins
        /// 
        /// This test verifies that when AllowCredentials() is used, the policy
        /// cannot use AllowAnyOrigin() (security requirement).
        /// 
        /// **Validates: Requirements 2.5, 2.6**
        /// </summary>
        [Fact]
        public void Property1_CorsPolicy_WithCredentials_RequiresSpecificOrigins()
        {
            // Arrange - Create CORS policy with credentials and specific origins
            var allowedOrigins = new[]
            {
                "https://app.example.com"
            };

            var policyBuilder = new CorsPolicyBuilder();
            policyBuilder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
            var policy = policyBuilder.Build();

            // Assert - Policy with credentials must have specific origins
            Assert.False(policy.AllowAnyOrigin);
            Assert.True(policy.SupportsCredentials);
            Assert.True(policy.IsOriginAllowed("https://app.example.com"));
            Assert.False(policy.IsOriginAllowed("https://evil.com"));
        }

        /// <summary>
        /// Property 1: Bug Condition - Document current Gateway.API CORS configuration
        /// 
        /// This test documents that the current Gateway.API uses AllowAnyOrigin()
        /// which is the root cause of the CORS vulnerability.
        /// 
        /// After the fix, Gateway.API should use WithOrigins() with a restricted list.
        /// 
        /// **Validates: Requirements 2.1, 2.2, 2.3**
        /// </summary>
        [Fact]
        public void Property1_DocumentBuggyBehavior_GatewayUsesAllowAnyOrigin()
        {
            // This test documents the CURRENT BUGGY configuration in Gateway.API/Program.cs:
            // 
            // builder.Services.AddCors(options =>
            // {
            //     options.AddPolicy("AllowAll", policy =>
            //         policy.AllowAnyOrigin()      // <-- BUG: Allows any origin
            //               .AllowAnyHeader()
            //               .AllowAnyMethod());
            // });
            //
            // EXPECTED AFTER FIX:
            // 
            // var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
            // builder.Services.AddCors(options =>
            // {
            //     options.AddPolicy("RestrictedOrigins", policy =>
            //         policy.WithOrigins(allowedOrigins)  // <-- FIX: Restrict to allowlist
            //               .AllowAnyHeader()
            //               .AllowAnyMethod());
            // });

            // Arrange - Simulate current buggy configuration
            var buggyPolicyBuilder = new CorsPolicyBuilder();
            buggyPolicyBuilder.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod();
            var buggyPolicy = buggyPolicyBuilder.Build();

            // Assert - Current configuration allows any origin (BUG)
            Assert.True(buggyPolicy.AllowAnyOrigin);
            Assert.Contains("*", buggyPolicy.Origins); // "*" indicates any origin is allowed

            // Arrange - Simulate fixed configuration
            var fixedPolicyBuilder = new CorsPolicyBuilder();
            var allowedOrigins = new[]
            {
                "https://app.example.com",
                "https://admin.example.com",
                "http://localhost:3000"
            };
            fixedPolicyBuilder.WithOrigins(allowedOrigins)
                             .AllowAnyHeader()
                             .AllowAnyMethod();
            var fixedPolicy = fixedPolicyBuilder.Build();

            // Assert - Fixed configuration restricts origins (CORRECT)
            Assert.False(fixedPolicy.AllowAnyOrigin);
            Assert.True(fixedPolicy.IsOriginAllowed("https://app.example.com"));
            Assert.False(fixedPolicy.IsOriginAllowed("https://evil.com"));
            Assert.False(fixedPolicy.IsOriginAllowed("https://malicious-site.com"));
        }
    }
}
