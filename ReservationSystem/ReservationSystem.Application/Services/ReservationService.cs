using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Application.Services;

public class ReservationService
{
    private readonly IReservationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReservationConflictChecker _conflictChecker;

    public ReservationService(
        IReservationRepository repository,
        IUnitOfWork unitOfWork,
        ReservationConflictChecker conflictChecker)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _conflictChecker = conflictChecker;
    }

    public async Task CreateReservationAsync(
        Guid specialistId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginSerializableTransactionAsync(cancellationToken);

            var existingReservations = await _repository.GetActiveForSpecialistAsync(
                specialistId,
                startTime,
                endTime,
                cancellationToken);

            var reservation = new Reservation(
                specialistId,
                userId,
                startTime,
                endTime);

            if (_conflictChecker.HasConflict(reservation, existingReservations))
                throw new InvalidOperationException("Reservation time conflict.");

            await _repository.AddAsync(reservation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}