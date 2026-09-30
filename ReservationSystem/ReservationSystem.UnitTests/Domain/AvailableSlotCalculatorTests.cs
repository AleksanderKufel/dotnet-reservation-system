using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Tests.Domain;

public class AvailableSlotCalculatorTests
{
    // Monday
    private static readonly DateOnly Date = new(2030, 1, 7);
    private static readonly DateTime DayBefore = new(2030, 1, 6, 12, 0, 0, DateTimeKind.Utc);

    private readonly AvailableSlotCalculator _calculator = new();

    [Fact]
    public void GetAvailableSlots_ShouldReturnHourlySlotsWithinWorkingHours_WhenDayIsFree()
    {
        var slots = _calculator.GetAvailableSlots(Date, [], DayBefore);

        Assert.Equal(8, slots.Count);
        Assert.Equal(At(9), slots[0].StartTime);
        Assert.Equal(At(17), slots[^1].EndTime);
    }

    [Fact]
    public void GetAvailableSlots_ShouldReturnNoSlots_OnWeekend()
    {
        var saturday = new DateOnly(2030, 1, 5);

        var slots = _calculator.GetAvailableSlots(saturday, [], DayBefore);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetAvailableSlots_ShouldSkipSlotsOverlappingActiveReservations()
    {
        var reservations = new[] { Reserve(At(10).AddMinutes(30), At(11).AddMinutes(30)) };

        var slots = _calculator.GetAvailableSlots(Date, reservations, DayBefore);

        Assert.DoesNotContain(slots, s => s.StartTime == At(10));
        Assert.DoesNotContain(slots, s => s.StartTime == At(11));
        Assert.Equal(6, slots.Count);
    }

    [Fact]
    public void GetAvailableSlots_ShouldIncludeSlotsOfCancelledReservations()
    {
        var reservation = Reserve(At(10), At(11));
        reservation.Cancel(DayBefore, TimeSpan.Zero);

        var slots = _calculator.GetAvailableSlots(Date, [reservation], DayBefore);

        Assert.Contains(slots, s => s.StartTime == At(10));
    }

    [Fact]
    public void GetAvailableSlots_ShouldSkipSlotsThatAlreadyStarted()
    {
        var now = At(12).AddMinutes(30);

        var slots = _calculator.GetAvailableSlots(Date, [], now);

        Assert.Equal(At(13), slots[0].StartTime);
    }

    private static DateTime At(int hour) => Date.ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc);

    private static Reservation Reserve(DateTime start, DateTime end) =>
        new(Guid.NewGuid(), Guid.NewGuid(), start, end);
}
