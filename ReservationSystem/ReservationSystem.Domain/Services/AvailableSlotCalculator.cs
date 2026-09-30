using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;

namespace ReservationSystem.Domain.Services;

public class AvailableSlotCalculator
{
    public IReadOnlyList<TimeSlot> GetAvailableSlots(
        DateOnly date,
        IEnumerable<Reservation> reservations,
        DateTime now)
    {
        if (!WorkingHours.IsWorkingDay(date))
            return [];

        var activeReservations = reservations
            .Where(r => r.Status == ReservationStatus.Active)
            .ToList();

        var slots = new List<TimeSlot>();
        var dayEnd = date.ToDateTime(WorkingHours.DayEnd, DateTimeKind.Utc);

        for (var start = date.ToDateTime(WorkingHours.DayStart, DateTimeKind.Utc);
             start + WorkingHours.SlotLength <= dayEnd;
             start += WorkingHours.SlotLength)
        {
            var end = start + WorkingHours.SlotLength;

            var isTaken = activeReservations.Any(r => start < r.EndTime && end > r.StartTime);

            if (start > now && !isTaken)
                slots.Add(new TimeSlot(start, end));
        }

        return slots;
    }
}
