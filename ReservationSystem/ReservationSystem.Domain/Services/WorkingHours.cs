namespace ReservationSystem.Domain.Services;

// Shared by all specialists for now. Times are in UTC.
public static class WorkingHours
{
    public static readonly TimeOnly DayStart = new(9, 0);
    public static readonly TimeOnly DayEnd = new(17, 0);
    public static readonly TimeSpan SlotLength = TimeSpan.FromHours(1);

    public static bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

    public static bool MatchesSlot(DateTime startTime, DateTime endTime)
    {
        var date = DateOnly.FromDateTime(startTime);
        var dayStart = date.ToDateTime(DayStart);

        return IsWorkingDay(date)
            && endTime - startTime == SlotLength
            && startTime >= dayStart
            && endTime <= date.ToDateTime(DayEnd)
            && (startTime - dayStart).Ticks % SlotLength.Ticks == 0;
    }
}
