using AssignmentSystem.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AssignmentSystem.UnitTests.Integration;

/// <summary>
/// Boots the real Program.cs pipeline (auth, routing, middleware) against a SQLite in-memory DB
/// instead of Postgres. Program.cs's own startup migrate+seed still runs against this swapped-in
/// connection, so the factory ends up with the same seeded demo accounts used for manual Swagger testing.
/// </summary>
public class AssignmentSystemWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AssignmentSystemWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Plain RemoveAll<DbContextOptions<AppDbContext>>() isn't enough: AddDbContext registers
            // several supporting descriptors (options configuration, provider services) that accumulate
            // across repeated AddDbContext calls for the same TContext rather than being replaced, which
            // leaves the original Npgsql provider registered alongside the Sqlite one added here. Sweep
            // anything related to AppDbContext/Npgsql out before re-registering cleanly.
            var toRemove = services.Where(d =>
                d.ServiceType.FullName?.Contains("AppDbContext") == true ||
                d.ServiceType.FullName?.Contains("Npgsql") == true).ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
                // The migration snapshot was generated against Npgsql; provider-specific relational
                // annotations differ enough from Sqlite's that EF's model-diff check (added in EF Core 8)
                // flags a false-positive "pending changes" here. The actual OnModelCreating config is
                // unchanged, so it's safe to suppress for this swapped-provider test context.
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
