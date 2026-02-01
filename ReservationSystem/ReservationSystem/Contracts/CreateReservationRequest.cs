namespace ReservationSystem.Api.Contracts;

public class CreateReservationRequest
{
    public Guid SpecialistId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}