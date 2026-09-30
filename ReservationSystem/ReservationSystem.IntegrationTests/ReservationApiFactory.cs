using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Infrastructure.Persistence;
using Respawn;
using Testcontainers.PostgreSql;

namespace ReservationSystem.IntegrationTests;

public sealed class ReservationApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder("postgres:17-alpine").Build();

    private Respawner _respawner = default!;

    // Monday 8:00 UTC of next week: before opening, so every slot of that week is in the future.
    public FakeTimeProvider Clock { get; } = new(StartOfNextWeek());

    public FakePublicHolidayProvider PublicHolidays { get; } = new();

    public DateTime SlotStart(int daysAfterMonday, int hour) =>
        Clock.GetUtcNow().UtcDateTime.Date.AddDays(daysAfterMonday).AddHours(hour);

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
        await dbContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            // Specialists are seed data from migrations, so they are kept between tests.
            TablesToIgnore = ["__EFMigrationsHistory", "Specialists"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _database.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);

            services.RemoveAll<IPublicHolidayProvider>();
            services.AddSingleton<IPublicHolidayProvider>(PublicHolidays);
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static DateTimeOffset StartOfNextWeek()
    {
        var monday = DateTime.UtcNow.Date.AddDays(1);

        while (monday.DayOfWeek != DayOfWeek.Monday)
            monday = monday.AddDays(1);

        return new DateTimeOffset(monday.AddHours(8), TimeSpan.Zero);
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection
    : ICollectionFixture<ReservationApiFactory>
{
    public const string Name = "Integration";
}
