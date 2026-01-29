using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Application.Services;

public class ReservationService
{
    private readonly IReservationRepository _repository;
    private readonly ReservationConflictChecker _conflictChecker;

    public ReservationService(
        IReservationRepository repository,
        ReservationConflictChecker conflictChecker)
    {
        _repository = repository;
        _conflictChecker = conflictChecker;
    }

    public async Task CreateReservationAsync(
        Guid specialistId,
        Guid userId,
        DateTime startTime,
        DateTime endTime)
    {
        var existingReservations = await _repository.GetActiveForSpecialistAsync(
            specialistId,
            startTime,
            endTime);

        var reservation = new Reservation(
            specialistId,
            userId,
            startTime,
            endTime);

        if (_conflictChecker.HasConflict(reservation, existingReservations))
            throw new InvalidOperationException("Reservation time conflict.");

        await _repository.AddAsync(reservation);
    }
}