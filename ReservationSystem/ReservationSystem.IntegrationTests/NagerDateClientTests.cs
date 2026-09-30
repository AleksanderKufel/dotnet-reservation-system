using FluentAssertions;
using Microsoft.Extensions.Options;
using ReservationSystem.Infrastructure.PublicHolidays;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace ReservationSystem.IntegrationTests;

public sealed class NagerDateClientTests : IDisposable
{
    // Listen on loopback only, so the test doesn't open a port to the network.
    private readonly WireMockServer _server = WireMockServer.Start(new WireMockServerSettings
    {
        Urls = ["http://127.0.0.1:0"]
    });

    private readonly HttpClient _httpClient;

    public NagerDateClientTests()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };
    }

    [Fact]
    public async Task GetPublicHolidays_Returns_Only_Nationwide_Holidays()
    {
        _server
            .Given(Request.Create().WithPath("/api/v3/PublicHolidays/2030/PL").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                    [
                      { "date": "2030-01-01", "name": "New Year's Day", "global": true },
                      { "date": "2030-05-03", "name": "Regional Day", "global": false },
                      { "date": "2030-11-11", "name": "Independence Day", "global": true }
                    ]
                    """));

        var holidays = await CreateClient().GetPublicHolidaysAsync(2030);

        holidays.Should().Equal(new DateOnly(2030, 1, 1), new DateOnly(2030, 11, 11));
    }

    private NagerDateClient CreateClient() =>
        new(
            _httpClient,
            Options.Create(new PublicHolidaysOptions { BaseUrl = _server.Url!, CountryCode = "PL" }));

    public void Dispose()
    {
        _httpClient.Dispose();
        _server.Stop();
    }
}
