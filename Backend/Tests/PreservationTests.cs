using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Backend.Tests
{
    /// <summary>
    /// Preservation tests to ensure functional behavior remains unchanged after code quality fixes.
    /// These tests verify that authentication, authorization, routing, and other core functionality
    /// continue to work correctly after removing comments and adding missing configurations.
    /// EXPECTED: All tests should PASS on both unfixed and fixed code.
    /// </summary>
    public class PreservationTests
    {
        private readonly string _backendRoot;

        public PreservationTests()
        {
            _backendRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        }

        [Fact]
        public void Preservation1_AllServices_ShouldHave_JWTAuthentication()
        {
            // Arrange
            var serviceFiles = new[]
            {
                Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify JWT authentication is configured
                Assert.Contains("AddAuthentication", content);
                Assert.Contains("JwtBearerDefaults.AuthenticationScheme", content);
                Assert.Contains("TokenValidationParameters", content);
                Assert.Contains("ValidateIssuerSigningKey = true", content);
            }
        }

        [Fact]
        public void Preservation2_AllServices_ShouldHave_AuthorizationMiddleware()
        {
            // Arrange
            var serviceFiles = new[]
            {
                Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify authorization middleware is used
                Assert.Contains("UseAuthentication()", content);
                Assert.Contains("UseAuthorization()", content);
            }
        }

        [Fact]
        public void Preservation3_GatewayAPI_ShouldHave_ExceptionHandlerMiddleware()
        {
            // Arrange
            var gatewayProgramFile = Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs");
            var content = File.ReadAllText(gatewayProgramFile);

            // Act & Assert
            // Verify exception handler middleware is registered
            Assert.Contains("UseMiddleware<GlobalExceptionHandlerMiddleware>", content);
        }

        [Fact]
        public void Preservation4_GatewayAPI_ShouldHave_CORSConfiguration()
        {
            // Arrange
            var gatewayProgramFile = Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs");
            var content = File.ReadAllText(gatewayProgramFile);

            // Act & Assert
            // Verify CORS is configured with restricted origins (after security fix)
            Assert.Contains("AddCors", content);
            Assert.Contains("UseCors", content);
            Assert.Contains("RestrictedOrigins", content);
            Assert.Contains("WithOrigins", content);
        }

        [Fact]
        public void Preservation5_AllServices_ShouldHave_SwaggerConfiguration()
        {
            // Arrange
            var serviceFiles = new[]
            {
                Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify Swagger is configured with JWT bearer support
                Assert.Contains("AddSwaggerGen", content);
                Assert.Contains("AddSecurityDefinition", content);
                Assert.Contains("Bearer", content);
            }
        }

        [Fact]
        public void Preservation6_WorkflowAPI_ShouldHave_MassTransitPublisher()
        {
            // Arrange
            var workflowProgramFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs");
            var content = File.ReadAllText(workflowProgramFile);

            // Act & Assert
            // Verify MassTransit is configured for publishing
            Assert.Contains("AddMassTransit", content);
            Assert.Contains("UsingRabbitMq", content);
        }

        [Fact]
        public void Preservation7_ReportingAPI_ShouldHave_MassTransitConsumer()
        {
            // Arrange
            var reportingProgramFile = Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs");
            var content = File.ReadAllText(reportingProgramFile);

            // Act & Assert
            // Verify MassTransit consumer is configured
            Assert.Contains("AddMassTransit", content);
            Assert.Contains("AddConsumer<ProductStatusChangedConsumer>", content);
            Assert.Contains("ReceiveEndpoint", content);
            Assert.Contains("reporting_audit_queue", content);
        }

        [Fact]
        public void Preservation8_WorkflowController_ShouldHave_AuthorizeAttributes()
        {
            // Arrange
            var workflowControllerFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Controllers", "WorkflowController.cs");
            var content = File.ReadAllText(workflowControllerFile);

            // Act & Assert
            // Verify authorization attributes are present
            Assert.Contains("[Authorize]", content);
            Assert.Contains("[Authorize(Roles = \"Admin\")]", content);
        }

        [Fact]
        public void Preservation9_WorkflowController_ShouldHave_AllEndpoints()
        {
            // Arrange
            var workflowControllerFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Controllers", "WorkflowController.cs");
            var content = File.ReadAllText(workflowControllerFile);

            // Act & Assert
            // Verify all endpoints are present with correct routes
            Assert.Contains("[HttpPut(\"products/{id:guid}/pricing\")]", content);
            Assert.Contains("[HttpPut(\"products/{id:guid}/inventory\")]", content);
            Assert.Contains("[HttpPost(\"products/{id:guid}/submit\")]", content);
            Assert.Contains("[HttpPut(\"products/{id:guid}/status\")]", content);
            
            // Verify method names
            Assert.Contains("UpdatePricing", content);
            Assert.Contains("UpdateInventory", content);
            Assert.Contains("SubmitForReview", content);
            Assert.Contains("UpdateStatus", content);
        }

        [Fact]
        public void Preservation10_WorkflowService_ShouldPublish_StatusChangedEvent()
        {
            // Arrange
            var workflowServiceFile = Path.Combine(_backendRoot, "Services", "Workflow.API", "Application", "Services", "WorkflowService.cs");
            var content = File.ReadAllText(workflowServiceFile);

            // Act & Assert
            // Verify event publishing logic is present
            Assert.Contains("_publishEndpoint.Publish", content);
            Assert.Contains("ProductStatusChangedEvent", content);
            Assert.Contains("ProductId", content);
            Assert.Contains("OldStatus", content);
            Assert.Contains("NewStatus", content);
            Assert.Contains("ChangedByUserId", content);
        }

        [Fact]
        public void Preservation11_ProductStatusChangedConsumer_ShouldConsume_Events()
        {
            // Arrange
            var consumerFile = Path.Combine(_backendRoot, "Services", "Reporting.API", "Application", "Consumers", "ProductStatusChangedConsumer.cs");
            var content = File.ReadAllText(consumerFile);

            // Act & Assert
            // Verify consumer implements IConsumer interface
            Assert.Contains("IConsumer<ProductStatusChangedEvent>", content);
            Assert.Contains("Consume", content);
            Assert.Contains("AddAuditLogAsync", content);
        }

        [Fact]
        public void Preservation12_AllServices_ShouldHave_SerilogConfiguration()
        {
            // Arrange
            var serviceFiles = new[]
            {
                Path.Combine(_backendRoot, "ApiGateways", "Gateway.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"),
                Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs")
            };

            // Act & Assert
            foreach (var file in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify Serilog is configured
                Assert.Contains("Log.Logger", content);
                Assert.Contains("UseSerilog", content);
                Assert.Contains("UseSerilogRequestLogging", content);
            }
        }

        [Fact]
        public void Preservation13_AllServices_ShouldHave_DatabaseConfiguration()
        {
            // Arrange
            var serviceFiles = new[]
            {
                (Path.Combine(_backendRoot, "Services", "Identity.API", "Program.cs"), "IdentityDBContext"),
                (Path.Combine(_backendRoot, "Services", "Catalog.API", "Program.cs"), "CatalogDbContext"),
                (Path.Combine(_backendRoot, "Services", "Workflow.API", "Program.cs"), "WorkflowDbContext"),
                (Path.Combine(_backendRoot, "Services", "Reporting.API", "Program.cs"), "ReportingDbContext")
            };

            // Act & Assert
            foreach (var (file, dbContextName) in serviceFiles)
            {
                var content = File.ReadAllText(file);
                
                // Verify database context is configured
                Assert.Contains("AddDbContext", content);
                Assert.Contains(dbContextName, content);
                Assert.Contains("UseSqlServer", content);
            }
        }

        [Fact]
        public void Preservation14_ProductStatusChangedEvent_ShouldHave_AllProperties()
        {
            // Arrange
            var eventFile = Path.Combine(_backendRoot, "Shared", "Shared.Messaging", "ProductStatusChangedEvent.cs");
            var content = File.ReadAllText(eventFile);

            // Act & Assert
            // Verify all properties exist
            Assert.Contains("public Guid ProductId", content);
            Assert.Contains("public string OldStatus", content);
            Assert.Contains("public string NewStatus", content);
            Assert.Contains("public Guid ChangedByUserId", content);
        }

        [Fact]
        public void Preservation15_AuthController_ShouldHave_AllEndpoints()
        {
            // Arrange
            var authControllerFile = Path.Combine(_backendRoot, "Services", "Identity.API", "Controllers", "AuthController.cs");
            var content = File.ReadAllText(authControllerFile);

            // Act & Assert
            // Verify all authentication endpoints are present
            Assert.Contains("[HttpPost(\"register\")]", content);
            Assert.Contains("[HttpPost(\"login\")]", content);
            Assert.Contains("[HttpPost(\"refresh\")]", content);
            Assert.Contains("[HttpPost(\"logout\")]", content);
            Assert.Contains("[HttpPost(\"forgot-password\")]", content);
            Assert.Contains("[HttpPost(\"reset-password\")]", content);
            Assert.Contains("[HttpPost(\"change-password\")]", content);
            Assert.Contains("[HttpGet(\"profile\")]", content);
            Assert.Contains("[HttpGet(\"admin\")]", content);
        }
    }
}
