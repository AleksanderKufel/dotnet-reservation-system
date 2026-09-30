using Microsoft.EntityFrameworkCore;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Infrastructure.Persistence;

namespace ReservationSystem.Infrastructure.Repositories;

public class SpecialistRepository : ISpecialistRepository
{
    private readonly ReservationDbContext _dbContext;

    public SpecialistRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Specialist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Specialists
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Specialists.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
