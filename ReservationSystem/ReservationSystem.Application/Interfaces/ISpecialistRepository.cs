using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Interfaces;

public interface ISpecialistRepository
{
    Task<IReadOnlyList<Specialist>> GetAllAsync(CancellationToken cancellationToken = default);
}
