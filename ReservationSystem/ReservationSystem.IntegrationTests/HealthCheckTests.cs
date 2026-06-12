using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace ReservationSystem.IntegrationTests;

public class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests()
    {
        var factory = new WebApplicationFactory<Program>();

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns_Ok()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}