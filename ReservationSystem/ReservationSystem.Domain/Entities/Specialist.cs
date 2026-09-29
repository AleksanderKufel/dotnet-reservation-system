namespace ReservationSystem.Domain.Entities;

public class Specialist
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Specialization { get; private set; } = default!;

    private Specialist() { }

    public Specialist(Guid id, string name, string specialization)
    {
        Id = id;
        Name = name;
        Specialization = specialization;
    }
}
