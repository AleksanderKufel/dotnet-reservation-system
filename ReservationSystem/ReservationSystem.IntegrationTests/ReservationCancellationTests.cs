using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Domain.Enums;
using ReservationSystem.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ReservationCancellationTests : IntegrationTestBase
{
    public ReservationCancellationTests(ReservationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Cancel_Own_Reservation_Returns_Cancelled_Reservation()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var reservationId = await CreateReservationAsync(client, DateTime.UtcNow.AddDays(3));

        var response = await client.PostAsync($"/api/reservations/{reservationId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelled = await response.Content.ReadFromJsonAsync<ReservationResponse>(TestJson.Options);

        cancelled!.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_Reservation_Too_Close_To_Start_Returns_BadRequest()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var reservationId = await CreateReservationAsync(client, DateTime.UtcNow.AddHours(2));

        var response = await client.PostAsync($"/api/reservations/{reservationId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_Reservation_Of_Another_User_Returns_NotFound()
    {
        var owner = Factory.CreateClient();
        var otherUser = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(owner);
        await TestAuthHelper.AuthenticateAsync(otherUser);

        var reservationId = await CreateReservationAsync(owner, DateTime.UtcNow.AddDays(3));

        var response = await otherUser.PostAsync($"/api/reservations/{reservationId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Guid> CreateReservationAsync(HttpClient client, DateTime startTime)
    {
        var response = await client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = startTime,
            EndTime = startTime.AddHours(1)
        });

        response.EnsureSuccessStatusCode();

        var reservation = await response.Content.ReadFromJsonAsync<ReservationResponse>(TestJson.Options);

        return reservation!.Id;
    }
}
