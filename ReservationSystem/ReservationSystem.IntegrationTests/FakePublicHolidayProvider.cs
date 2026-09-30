using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;

namespace ReservationSystem.IntegrationTests;

// Replaces the Nager.Date client in API tests, so they never call the internet.
public sealed class FakePublicHolidayProvider : IPublicHolidayProvider
{
    public HashSet<DateOnly> Holidays { get; } = [];

    public bool IsUnavailable { get; set; }

    public Task<bool> IsPublicHolidayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        if (IsUnavailable)
            throw new PublicHolidaysUnavailableException("Public holidays are unavailable.", new HttpRequestException());

        return Task.FromResult(Holidays.Contains(date));
    }

    public void Reset()
    {
        Holidays.Clear();
        IsUnavailable = false;
    }
}
