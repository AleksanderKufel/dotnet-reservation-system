using FluentAssertions;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Infrastructure.Persistence;
using System.Net.Http.Json;

namespace ReservationSystem.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class SpecialistsTests
{
    private readonly HttpClient _client;

    public SpecialistsTests(ReservationApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Specialists_Returns_Seeded_Specialists_Without_Authentication()
    {
        var specialists = await _client.GetFromJsonAsync<List<SpecialistResponse>>("/api/specialists");

        specialists.Should().NotBeNull();
        specialists!.Select(s => s.Id).Should().BeEquivalentTo(SpecialistSeed.All.Select(s => s.Id));
    }
}
