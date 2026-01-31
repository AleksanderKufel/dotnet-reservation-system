using Microsoft.EntityFrameworkCore;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;
using ReservationSystem.Infrastructure.Persistence;

namespace ReservationSystem.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _dbContext;

    public ReservationRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Reservation>> GetActiveForSpecialistAsync(
        Guid specialistId,
        DateTime from,
        DateTime to)
    {
        return await _dbContext.Reservations
            .Where(r =>
                r.SpecialistId == specialistId &&
                r.Status == ReservationStatus.Active &&
                r.StartTime < to &&
                r.EndTime > from)
            .ToListAsync();
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _dbContext.Reservations.AddAsync(reservation);
        await _dbContext.SaveChangesAsync();
    }
}