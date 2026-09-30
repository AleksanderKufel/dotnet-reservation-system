using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Application.Services;

public class SpecialistService
{
    private readonly ISpecialistRepository _repository;
    private readonly IReservationRepository _reservationRepository;
    private readonly AvailableSlotCalculator _slotCalculator;
    private readonly TimeProvider _timeProvider;

    public SpecialistService(
        ISpecialistRepository repository,
        IReservationRepository reservationRepository,
        AvailableSlotCalculator slotCalculator,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _reservationRepository = reservationRepository;
        _slotCalculator = slotCalculator;
        _timeProvider = timeProvider;
    }

    public Task<IReadOnlyList<Specialist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeSlot>> GetAvailableSlotsAsync(
        Guid specialistId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(specialistId, cancellationToken))
            throw new NotFoundException("Specialist not found.");

        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var reservations = await _reservationRepository.GetActiveForSpecialistAsync(
            specialistId,
            dayStart,
            dayStart.AddDays(1),
            cancellationToken);

        return _slotCalculator.GetAvailableSlots(
            date,
            reservations,
            _timeProvider.GetUtcNow().UtcDateTime);
    }
}
