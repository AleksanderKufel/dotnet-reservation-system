using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence;

public static class SpecialistSeed
{
    public static readonly Guid AnnaNowakId = new("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e01");
    public static readonly Guid PiotrKowalskiId = new("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e02");
    public static readonly Guid MartaWisniewskaId = new("3f1c9a52-6b1e-4f5a-9d2e-1a7b8c9d0e03");

    public static IReadOnlyList<Specialist> All { get; } =
    [
        new(AnnaNowakId, "Anna Nowak", "Family physician"),
        new(PiotrKowalskiId, "Piotr Kowalski", "Pediatrician"),
        new(MartaWisniewskaId, "Marta Wiśniewska", "Dermatologist")
    ];
}
