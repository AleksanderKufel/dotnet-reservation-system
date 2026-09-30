namespace ReservationSystem.Domain.Services;

// Shared by all specialists for now. Times are in UTC.
public static class WorkingHours
{
    public static readonly TimeOnly DayStart = new(9, 0);
    public static readonly TimeOnly DayEnd = new(17, 0);
    public static readonly TimeSpan SlotLength = TimeSpan.FromHours(1);

    public static bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
}
