using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Infrastructure.PublicHolidays;

namespace ReservationSystem.IntegrationTests;

// Calls the real date.nager.at API. Excluded from PR builds, run weekly by external-api-smoke.yml.
[Trait("Category", "External")]
public class NagerDateSmokeTests
{
    [Fact]
    public async Task Real_Api_Returns_Polish_Independence_Day()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicHolidays:BaseUrl"] = "https://date.nager.at/",
                ["PublicHolidays:CountryCode"] = "PL"
            })
            .Build();

        await using var services = new ServiceCollection()
            .AddPublicHolidays(configuration)
            .BuildServiceProvider();

        var provider = services.GetRequiredService<IPublicHolidayProvider>();
        var year = DateTime.UtcNow.Year;

        (await provider.IsPublicHolidayAsync(new DateOnly(year, 11, 11))).Should().BeTrue();
        (await provider.IsPublicHolidayAsync(new DateOnly(year, 11, 10))).Should().BeFalse();
    }
}
