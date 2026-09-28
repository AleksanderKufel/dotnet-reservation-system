using FluentAssertions;
using ReservationSystem.Api.Contracts;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ReservationCreationTests : IntegrationTestBase
{
    public ReservationCreationTests(ReservationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Reservation_Returns_Created()
    {
        var client = Factory.CreateClient();

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