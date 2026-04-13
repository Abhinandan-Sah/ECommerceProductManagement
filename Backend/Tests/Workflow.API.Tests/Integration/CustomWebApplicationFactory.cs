using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workflow.API.Infrastructure.Data;

namespace Workflow.API.Tests.Integration
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all EF Core related services
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType.Namespace != null && 
                               (d.ServiceType.Namespace.Contains("EntityFrameworkCore") ||
                                d.ServiceType == typeof(WorkflowDbContext) ||
                                d.ServiceType == typeof(DbContextOptions<WorkflowDbContext>)))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using in-memory database for testing
                services.AddDbContext<WorkflowDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        }
    }
}
