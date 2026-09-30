using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ReservationSystem.Infrastructure.PublicHolidays;

// Typed client for https://date.nager.at
public class NagerDateClient
{
    private readonly HttpClient _httpClient;
    private readonly PublicHolidaysOptions _options;

    public NagerDateClient(HttpClient httpClient, IOptions<PublicHolidaysOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<DateOnly>> GetPublicHolidaysAsync(int year, CancellationToken cancellationToken = default)
    {
        var holidays = await _httpClient.GetFromJsonAsync<List<NagerPublicHoliday>>(
            $"api/v3/PublicHolidays/{year}/{_options.CountryCode}",
            cancellationToken);

        // Regional holidays (Global = false) don't close the whole country.
        return holidays!
            .Where(h => h.Global)
            .Select(h => h.Date)
            .ToList();
    }

    private sealed record NagerPublicHoliday(DateOnly Date, string Name, bool Global);
}
