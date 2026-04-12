using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Backend.Tests
{
    /// <summary>
    /// Bug condition exploration tests for code quality and security issues.
    /// These tests verify that the bugs exist in the UNFIXED code.
    /// EXPECTED: All tests should FAIL on unfixed code (proving bugs exist).
    /// After fixes are applied, these same tests should PASS.
    /// </summary>
    public class CodeQualityBugConditionTests
    {
        private readonly string _backendRoot;

        public CodeQualityBugConditionTests()
        {
            // Navigate to Backend directory from test location
            _backendRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        }

        [Fact]
        public void Test1_DecorativeComments_ShouldNotExist_InProgramFiles()
        {
            // Arrange
            var programFiles = new[]
            {
                Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in programFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify NO decorative separator comments exist
                Assert.DoesNotContain("// ─────────────────────────────────────────", content);
                Assert.DoesNotContain("// ───", content);
            }
        }

        [Fact]
        public void Test2_CatalogAPI_ShouldHave_AddAuthorizationCall()
        {
            // Arrange
            var catalogProgramFile = Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs");
            var content = File.ReadAllText(catalogProgramFile);

            // Act & Assert
            // Verify AddAuthorization() is present
            Assert.Contains("AddAuthorization()", content);
        }

        [Fact]
        public void Test3_JWTClockSkew_ShouldBeConfigured_InAllServices()
        {
            // Arrange
            var serviceFiles = new[]
            {
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify ClockSkew = TimeSpan.Zero is present
                Assert.Contains("ClockSkew = TimeSpan.Zero", content);
            }
        }

        [Fact]
        public void Test4_WorkflowController_ShouldUse_ExtensionMethod_NotPrivateHelper()
        {
            // Arrange
            var workflowControllerFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Controllers", "WorkflowController.cs");
            var content = File.ReadAllText(workflowControllerFile);

            // Act & Assert
            // Verify NO private GetUserId() method exists
            Assert.DoesNotContain("private Guid GetUserId()", content);
            
            // Verify extension method pattern is used
            Assert.Contains("User.GetUserId()", content);
            Assert.Contains("using Workflow.API.Application.Extensions;", content);
        }

        [Fact]
        public void Test5_GlobalExceptionHandler_ShouldOnly_ExposeDeveloperDetails_InDevelopment()
        {
            // Arrange
            var middlewareFile = Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Middleware", "GlobalExceptionHandlerMiddleware.cs");
            var content = File.ReadAllText(middlewareFile);

            // Act & Assert
            // Verify environment check exists for DeveloperDetails
            Assert.Contains("IsDevelopment()", content);
            
            // Verify DeveloperDetails is conditionally included
            var lines = content.Split('\n');
            var developerDetailsLine = lines.FirstOrDefault(l => l.Contains("DeveloperDetails"));
            
            if (developerDetailsLine != null)
            {
                // If DeveloperDetails exists, it should be within a conditional block
                var indexOfDeveloperDetails = Array.IndexOf(lines, developerDetailsLine);
                var precedingLines = string.Join("\n", lines.Take(indexOfDeveloperDetails).TakeLast(10));
                
                Assert.Contains("if", precedingLines.ToLower());
            }
        }

        [Fact]
        public void Test6_ProductStatusChangedEvent_ShouldUse_XMLDocumentation()
        {
            // Arrange
            var eventFile = Path.Combine(_backendRoot, "Shared", "Shared.Messaging", "ProductStatusChangedEvent.cs");
            var content = File.ReadAllText(eventFile);

            // Act & Assert
            // Verify XML summary tags are used, not inline comments
            Assert.Contains("/// <summary>", content);
            
            // Verify NO inline property comments exist
            var lines = content.Split('\n');
            var propertyLines = lines.Where(l => l.Contains("public Guid") || l.Contains("public string")).ToList();
            
            foreach (var propertyLine in propertyLines)
            {
                var index = Array.IndexOf(lines, propertyLine);
                if (index > 0)
                {
                    var previousLine = lines[index - 1].Trim();
                    // Previous line should be XML doc, not inline comment
                    Assert.DoesNotContain("//", previousLine.Replace("///", ""));
                }
            }
        }

        [Fact]
        public void Test7_ProductStatusChangedConsumer_Documentation_ShouldBe_Correct()
        {
            // Arrange
            var consumerFile = Path.Combine(_backendRoot, "Services", "Reporting.API", "Application", "Consumers", "ProductStatusChangedConsumer.cs");
            var content = File.ReadAllText(consumerFile);

            // Act & Assert
            // Verify documentation does NOT mention Catalog.API
            var summarySection = content.Substring(
                content.IndexOf("/// <summary>"),
                content.IndexOf("/// </summary>") - content.IndexOf("/// <summary>") + 15
            );
            
            Assert.DoesNotContain("Catalog.API", summarySection);
            Assert.Contains("Workflow.API", summarySection);
        }

        [Fact]
        public void Test8_Controllers_ShouldNotHave_RedundantRouteComments()
        {
            // Arrange
            var workflowControllerFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Controllers", "WorkflowController.cs");
            var authControllerFile = Path.Combine(_backendRoot, "Services", "Identity.API", "Controllers", "AuthController.cs");

            // Act & Assert
            var workflowContent = File.ReadAllText(workflowControllerFile);
            var authContent = File.ReadAllText(authControllerFile);

            // Verify NO redundant route comments like "// POST /api/workflow/..."
            Assert.DoesNotContain("// POST /api/workflow/", workflowContent);
            Assert.DoesNotContain("// PUT /api/workflow/", workflowContent);
            Assert.DoesNotContain("// POST /api/auth/", authContent);
            Assert.DoesNotContain("// GET /api/auth/", authContent);
            
            // Verify NO redundant authorization comments
            Assert.DoesNotContain("// Enforces that EVERY endpoint requires a valid JWT token", workflowContent);
            Assert.DoesNotContain("// Critical: Only Admins can approve/publish!", workflowContent);
        }

        [Fact]
        public void Test9_IdentityAPI_ShouldNotHave_CommentedOutCode()
        {
            // Arrange
            var identityProgramFile = Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs");
            var content = File.ReadAllText(identityProgramFile);

            // Act & Assert
            // Verify NO commented-out CORS configuration exists
            Assert.DoesNotContain("//builder.Services.AddCors", content);
        }

        [Fact]
        public void Test10_GatewayAPI_ShouldNotHave_NumberedStepComments()
        {
            // Arrange
            var gatewayProgramFile = Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs");
            var content = File.ReadAllText(gatewayProgramFile);

            // Act & Assert
            // Verify NO numbered step comments exist
            Assert.DoesNotContain("// 1. Load", content);
            Assert.DoesNotContain("// 2. Register", content);
            Assert.DoesNotContain("// 3. Execute", content);
        }
    }
}
