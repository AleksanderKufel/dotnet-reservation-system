using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;

namespace ReservationSystem.Api.Contracts;

public record ReservationResponse(
    Guid Id,
    Guid SpecialistId,
    DateTime StartTime,
    DateTime EndTime,
    ReservationStatus Status,
    DateTime CreatedAt)
{
    public static ReservationResponse FromDomain(Reservation reservation) => new(
        reservation.Id,
        reservation.SpecialistId,
        reservation.StartTime,
        reservation.EndTime,
        reservation.Status,
        reservation.CreatedAt);
}
