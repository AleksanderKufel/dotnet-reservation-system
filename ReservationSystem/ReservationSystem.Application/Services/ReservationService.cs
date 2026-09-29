using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Models;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Application.Services;

public class ReservationService
{
    private readonly IReservationRepository _repository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReservationConflictChecker _conflictChecker;

    public ReservationService(
        IReservationRepository repository,
        ISpecialistRepository specialistRepository,
        IUnitOfWork unitOfWork,
        ReservationConflictChecker conflictChecker)
    {
        _repository = repository;
        _specialistRepository = specialistRepository;
        _unitOfWork = unitOfWork;
        _conflictChecker = conflictChecker;
    }

    public async Task<Reservation> CreateReservationAsync(
        Guid specialistId,
        Guid userId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        if (!await _specialistRepository.ExistsAsync(specialistId, cancellationToken))
            throw new NotFoundException("Specialist not found.");

        await using (var transaction = await _unitOfWork.BeginSerializableTransactionAsync(cancellationToken))
        {
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
                throw new ReservationConflictException("Reservation time conflict.");

            await _repository.AddAsync(reservation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return reservation;
        }
    }

    public Task<PagedResult<Reservation>> GetPageForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetForUserAsync(userId, page, pageSize, cancellationToken);
    }

    public async Task<Reservation> GetForUserAsync(
        Guid reservationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _repository.GetByIdAsync(reservationId, cancellationToken);

        // Another user's reservation is reported as missing, so its id cannot be probed.
        if (reservation is null || reservation.UserId != userId)
            throw new NotFoundException("Reservation not found.");

        return reservation;
    }
}
