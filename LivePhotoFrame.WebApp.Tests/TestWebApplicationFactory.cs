using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LivePhotoFrame.WebApp.Data;

namespace LivePhotoFrame.WebApp.Tests;

/// <summary>
/// Custom WebApplicationFactory that swaps in an in-memory database
/// so tests don't require a real PostgreSQL/SQL Server instance.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations (Npgsql/SqlServer + options)
            var dbDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("DbContext") == true
                         || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                         || d.ImplementationType?.FullName?.Contains("Npgsql") == true
                         || d.ImplementationType?.FullName?.Contains("SqlServer") == true)
                .ToList();

            foreach (var d in dbDescriptors)
                services.Remove(d);

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });

        builder.UseEnvironment("Development");
    }
}
