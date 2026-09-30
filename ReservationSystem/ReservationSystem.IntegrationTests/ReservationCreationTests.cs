using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Domain.Enums;
using ReservationSystem.Infrastructure.Persistence;
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
    public async Task Create_Reservation_Returns_Created_With_Location_And_Body()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var request = new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = Factory.SlotStart(1, 10),
            EndTime = Factory.SlotStart(1, 11)
        };

        var response =
            await client.PostAsJsonAsync(
                "/api/reservations",
                request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReservationResponse>(TestJson.Options);

        created!.SpecialistId.Should().Be(request.SpecialistId);
        created.Status.Should().Be(ReservationStatus.Active);

        response.Headers.Location.Should().NotBeNull();

        var fetched = await client.GetFromJsonAsync<ReservationResponse>(
            response.Headers.Location, TestJson.Options);

        // PostgreSQL stores microseconds, .NET DateTime has 100 ns ticks.
        fetched.Should().BeEquivalentTo(created, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    public async Task Create_Reservation_For_Unknown_Specialist_Returns_NotFound()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var request = new CreateReservationRequest
        {
            SpecialistId = Guid.NewGuid(),
            StartTime = Factory.SlotStart(1, 10),
            EndTime = Factory.SlotStart(1, 11)
        };

        var response = await client.PostAsJsonAsync("/api/reservations", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Reservation_On_Weekend_Returns_BadRequest()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var saturday = Factory.SlotStart(5, 10);

        var response = await client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = saturday,
            EndTime = saturday.AddHours(1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Reservation_Of_Another_User_Returns_NotFound()
    {
        var owner = Factory.CreateClient();
        var otherUser = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(owner);
        await TestAuthHelper.AuthenticateAsync(otherUser);

        var response = await owner.PostAsJsonAsync("/api/reservations", new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = Factory.SlotStart(1, 10),
            EndTime = Factory.SlotStart(1, 11)
        });

        response.EnsureSuccessStatusCode();

        var otherUserResponse = await otherUser.GetAsync(response.Headers.Location);

        otherUserResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
