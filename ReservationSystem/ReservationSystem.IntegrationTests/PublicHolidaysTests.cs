using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Infrastructure.PublicHolidays;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace ReservationSystem.IntegrationTests;

// Real HTTP client, resilience and cache, with WireMock in place of date.nager.at.
public sealed class PublicHolidaysTests : IDisposable
{
    private const string HolidaysPath = "/api/v3/PublicHolidays/2030/PL";

    private const string HolidaysJson = """
        [
          { "date": "2030-01-01", "name": "New Year's Day", "global": true },
          { "date": "2030-05-03", "name": "Regional Day", "global": false },
          { "date": "2030-11-11", "name": "Independence Day", "global": true }
        ]
        """;

    // Listen on loopback only, so the test doesn't open a port to the network.
    private readonly WireMockServer _server = WireMockServer.Start(new WireMockServerSettings
    {
        Urls = ["http://127.0.0.1:0"]
    });

    private readonly ServiceProvider _services;

    public PublicHolidaysTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicHolidays:BaseUrl"] = _server.Url,
                ["PublicHolidays:CountryCode"] = "PL"
            })
            .Build();

        _services = new ServiceCollection()
            .AddPublicHolidays(configuration)
            .BuildServiceProvider();
    }

    private IPublicHolidayProvider Provider => _services.GetRequiredService<IPublicHolidayProvider>();

    [Fact]
    public async Task Recognizes_Only_Nationwide_Holidays()
    {
        StubHolidays(200, HolidaysJson);

        (await Provider.IsPublicHolidayAsync(new DateOnly(2030, 11, 11))).Should().BeTrue();
        (await Provider.IsPublicHolidayAsync(new DateOnly(2030, 5, 3))).Should().BeFalse();
    }

    [Fact]
    public async Task Calls_Api_Once_Per_Year_Thanks_To_Cache()
    {
        StubHolidays(200, HolidaysJson);

        await Provider.IsPublicHolidayAsync(new DateOnly(2030, 1, 1));
        await Provider.IsPublicHolidayAsync(new DateOnly(2030, 6, 1));

        _server.LogEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task Retries_After_Server_Error()
    {
        _server
            .Given(Request.Create().WithPath(HolidaysPath).UsingGet())
            .InScenario("flaky")
            .WillSetStateTo("recovered")
            .RespondWith(Response.Create().WithStatusCode(500));

        _server
            .Given(Request.Create().WithPath(HolidaysPath).UsingGet())
            .InScenario("flaky")
            .WhenStateIs("recovered")
            .RespondWith(JsonResponse(200, HolidaysJson));

        var isHoliday = await Provider.IsPublicHolidayAsync(new DateOnly(2030, 11, 11));

        isHoliday.Should().BeTrue();
        _server.LogEntries.Should().HaveCount(2);
    }

    [Fact]
    public async Task Throws_Unavailable_When_Api_Keeps_Failing_And_Does_Not_Cache_Failure()
    {
        StubHolidays(500, "");

        var act = () => Provider.IsPublicHolidayAsync(new DateOnly(2030, 11, 11));

        await act.Should().ThrowAsync<PublicHolidaysUnavailableException>();

        _server.Reset();
        StubHolidays(200, HolidaysJson);

        (await Provider.IsPublicHolidayAsync(new DateOnly(2030, 11, 11))).Should().BeTrue();
    }

    private void StubHolidays(int statusCode, string body) =>
        _server
            .Given(Request.Create().WithPath(HolidaysPath).UsingGet())
            .RespondWith(JsonResponse(statusCode, body));

    private static IResponseBuilder JsonResponse(int statusCode, string body) =>
        Response.Create()
            .WithStatusCode(statusCode)
            .WithHeader("Content-Type", "application/json")
            .WithBody(body);

    public void Dispose()
    {
        _services.Dispose();
        _server.Stop();
    }
}
