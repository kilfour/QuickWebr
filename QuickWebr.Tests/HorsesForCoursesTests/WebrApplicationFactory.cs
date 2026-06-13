using HorsesForCourses.Api.Service.Warehouse;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuickPulse.Explains;

namespace QuickWebr.Tests.HorsesForCoursesTests;

[CodeExample]

public class WebrApplicationFactory
    : WebApplicationFactory<HorsesForCourses.Api.Program>
{
    private SqliteConnection connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.RemoveAll<AppDbContext>();

            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Auth:JwtKey"] = "a-very-long-random-secret-string-change-me",
                ["Auth:Issuer"] = "https://hfcc.example",
                ["Auth:Audience"] = "hfcc-api",
            };

            cfg.AddInMemoryCollection(overrides);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        connection.Dispose();
    }

    public EfReader GetReader() => new(Services);
}

public sealed class EfReader(IServiceProvider services)
{
    public T Query<T>(Func<AppDbContext, T> query)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return query(db);
    }
}
