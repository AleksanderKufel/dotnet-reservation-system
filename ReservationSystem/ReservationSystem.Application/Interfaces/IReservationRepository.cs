using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetActiveForSpecialistAsync(
        Guid specialistId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}