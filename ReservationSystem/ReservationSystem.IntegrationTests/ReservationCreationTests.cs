using FluentAssertions;
using ReservationSystem.Api.Contracts;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ReservationCreationTests
{
    private readonly ReservationApiFactory _factory;

    public ReservationCreationTests(
        ReservationApiFactory factory)
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