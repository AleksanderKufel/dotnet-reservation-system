using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class PublicHolidayReservationTests : IntegrationTestBase
{
    public PublicHolidayReservationTests(ReservationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_Reservation_On_Public_Holiday_Returns_BadRequest()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        var startTime = Factory.SlotStart(1, 10);
        Factory.PublicHolidays.Holidays.Add(DateOnly.FromDateTime(startTime));

        var response = await client.PostAsJsonAsync("/api/reservations", CreateRequest(startTime));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Slots_On_Public_Holiday_Returns_Empty_List()
    {
        var client = Factory.CreateClient();

        var date = DateOnly.FromDateTime(Factory.SlotStart(1, 0));
        Factory.PublicHolidays.Holidays.Add(date);

        var slots = await client.GetFromJsonAsync<List<TimeSlotResponse>>(
            $"/api/specialists/{SpecialistSeed.AnnaNowakId}/slots?date={date:yyyy-MM-dd}");

        slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_Reservation_When_Holidays_Are_Unavailable_Returns_ServiceUnavailable()
    {
        var client = Factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client);

        Factory.PublicHolidays.IsUnavailable = true;

        var response = await client.PostAsJsonAsync("/api/reservations", CreateRequest(Factory.SlotStart(1, 10)));

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    private static CreateReservationRequest CreateRequest(DateTime startTime) => new()
    {
        SpecialistId = SpecialistSeed.AnnaNowakId,
        StartTime = startTime,
        EndTime = startTime.AddHours(1)
    };
}
