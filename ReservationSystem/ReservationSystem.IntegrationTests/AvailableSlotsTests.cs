using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AvailableSlotsTests : IntegrationTestBase
{
    public AvailableSlotsTests(ReservationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get_Slots_Excludes_Reserved_Slot()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var date = NextMonday();
        var reservedStart = date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);

        var createResponse = await client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest
        {
            SpecialistId = SpecialistSeed.AnnaNowakId,
            StartTime = reservedStart,
            EndTime = reservedStart.AddHours(1)
        });

        createResponse.EnsureSuccessStatusCode();

        var slots = await client.GetFromJsonAsync<List<TimeSlotResponse>>(
            $"/api/specialists/{SpecialistSeed.AnnaNowakId}/slots?date={date:yyyy-MM-dd}");

        slots.Should().HaveCount(7);
        slots!.Select(s => s.StartTime).Should().NotContain(reservedStart);
    }

    [Fact]
    public async Task Get_Slots_For_Unknown_Specialist_Returns_NotFound()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/specialists/{Guid.NewGuid()}/slots?date={NextMonday():yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Slots_Without_Date_Returns_BadRequest()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/specialists/{SpecialistSeed.AnnaNowakId}/slots");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static DateOnly NextMonday()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);

        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(1);

        return date;
    }
}
