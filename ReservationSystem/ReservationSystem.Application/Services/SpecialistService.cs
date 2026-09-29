using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Application.Services;

public class SpecialistService
{
    private readonly ISpecialistRepository _repository;

    public SpecialistService(ISpecialistRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Specialist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }
}
