namespace ReservationSystem.Application.Interfaces;

public interface IPublicHolidayProvider
{
    Task<bool> IsPublicHolidayAsync(DateOnly date, CancellationToken cancellationToken = default);
}
