using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ReservationSystem.Api.Contracts;
using System.Net;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

public class BookingConcurrencyTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookingConcurrencyTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Not_Allow_Double_Booking()
    {
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        await TestAuthHelper.AuthenticateAsync(client1);
        await TestAuthHelper.AuthenticateAsync(client2);

        var request = new CreateReservationRequest
        {
            SpecialistId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow
                .AddDays(1)
                .Date
                .AddHours(10),
            EndTime = DateTime.UtcNow
                .AddDays(1)
                .Date
                .AddHours(11)
        };

        var barrier = new Barrier(2);

        var task1 = Task.Run(async () =>
        {
            barrier.SignalAndWait();

            return await client1.PostAsJsonAsync(
                "/api/reservations",
                request);
        });

        var task2 = Task.Run(async () =>
        {
            barrier.SignalAndWait();

            return await client2.PostAsJsonAsync(
                "/api/reservations",
                request);
        });

        await Task.WhenAll(task1, task2);

        var response1 = await task1;
        var response2 = await task2;

        var successCount = new[]
        {
            response1,
            response2
        }.Count(x =>
            x.StatusCode == HttpStatusCode.Created);

        var conflictCount = new[]
        {
            response1,
            response2
        }.Count(x =>
            x.StatusCode == HttpStatusCode.Conflict);

        Console.WriteLine(
    $"Response1: {(int)response1.StatusCode}");

        Console.WriteLine(
            await response1.Content.ReadAsStringAsync());

        Console.WriteLine(
            $"Response2: {(int)response2.StatusCode}");

        Console.WriteLine(
            await response2.Content.ReadAsStringAsync());
        successCount.Should().Be(1);

        conflictCount.Should().Be(1);

    }
}