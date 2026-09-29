using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class MyReservationsTests : IntegrationTestBase
{
    public MyReservationsTests(ReservationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get_My_Reservations_Returns_Only_Own_Reservations_Paged_Newest_First()
    {
        var client = Factory.CreateClient();
        var otherUser = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);
        await TestAuthHelper.AuthenticateAsync(otherUser);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        await CreateReservationAsync(client, tomorrow.AddHours(9));
        await CreateReservationAsync(client, tomorrow.AddHours(10));
        await CreateReservationAsync(client, tomorrow.AddHours(11));
        await CreateReservationAsync(otherUser, tomorrow.AddHours(12));

        var firstPage = await client.GetFromJsonAsync<PagedResponse<ReservationResponse>>(
            "/api/reservations/me?page=1&pageSize=2", TestJson.Options);

        var secondPage = await client.GetFromJsonAsync<PagedResponse<ReservationResponse>>(
            "/api/reservations/me?page=2&pageSize=2", TestJson.Options);

        firstPage!.TotalCount.Should().Be(3);
        firstPage.Items.Select(r => r.StartTime.Hour).Should().Equal(11, 10);
        secondPage!.Items.Select(r => r.StartTime.Hour).Should().Equal(9);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task Get_My_Reservations_With_Invalid_Paging_Returns_BadRequest(int page, int pageSize)
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var response = await client.GetAsync($"/api/reservations/me?page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task CreateReservationAsync(HttpClient client, DateTime startTime)
    {
        var response = await client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = startTime,
            EndTime = startTime.AddHours(1)
        });

        response.EnsureSuccessStatusCode();
    }
}
