using Microsoft.Extensions.Caching.Memory;
using Polly;
using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using System.Text.Json;

namespace ReservationSystem.Infrastructure.PublicHolidays;

public class PublicHolidayProvider : IPublicHolidayProvider
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly NagerDateClient _client;
    private readonly IMemoryCache _cache;

    public PublicHolidayProvider(NagerDateClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<bool> IsPublicHolidayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var holidays = await GetHolidaysAsync(date.Year, cancellationToken);

        return holidays.Contains(date);
    }

    private async Task<IReadOnlyList<DateOnly>> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        try
        {
            // A failed call throws, so nothing is cached and the next request tries the API again.
            return (await _cache.GetOrCreateAsync(
                $"public-holidays:{year}",
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                    return _client.GetPublicHolidaysAsync(year, cancellationToken);
                }))!;
        }
        catch (Exception exception) when (exception is HttpRequestException or ExecutionRejectedException or JsonException)
        {
            // ExecutionRejectedException covers resilience timeouts and an open circuit breaker.
            throw new PublicHolidaysUnavailableException("Public holidays are currently unavailable.", exception);
        }
    }
}
