using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ReservationSystem.Api.Contracts;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

public class ReservationCreationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReservationCreationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_Reservation_Returns_Created()
    {
        var client = _factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var request = new CreateReservationRequest
        {
            SpecialistId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/reservations",
                request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);
    }
}