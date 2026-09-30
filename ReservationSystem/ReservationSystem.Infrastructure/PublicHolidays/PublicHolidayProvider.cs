using ReservationSystem.Application.Interfaces;

namespace ReservationSystem.Infrastructure.PublicHolidays;

public class PublicHolidayProvider : IPublicHolidayProvider
{
    private readonly NagerDateClient _client;

    public PublicHolidayProvider(NagerDateClient client)
    {
        _client = client;
    }

    public async Task<bool> IsPublicHolidayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var holidays = await _client.GetPublicHolidaysAsync(date.Year, cancellationToken);

        return holidays.Contains(date);
    }
}
