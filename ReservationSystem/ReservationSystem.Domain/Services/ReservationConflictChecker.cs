using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;

namespace ReservationSystem.Domain.Services;

public class ReservationConflictChecker
{
    public bool HasConflict(
        Reservation newReservation,
        IEnumerable<Reservation> existingReservations)
    {
        return existingReservations.Any(existing =>
            existing.Status == ReservationStatus.Active &&
            newReservation.StartTime < existing.EndTime &&
            newReservation.EndTime > existing.StartTime);
    }
}