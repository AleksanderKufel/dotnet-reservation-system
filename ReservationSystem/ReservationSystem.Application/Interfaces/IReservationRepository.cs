using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetActiveForSpecialistAsync(
        Guid specialistId,
        DateTime from,
        DateTime to);

    Task AddAsync(Reservation reservation);
}