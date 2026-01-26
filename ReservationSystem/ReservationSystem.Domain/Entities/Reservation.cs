using ReservationSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationSystem.Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid SpecialistId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Reservation() { }

        public Reservation(
            Guid specialistId,
            Guid userId,
            DateTime startTime,
            DateTime endTime)
        {
            if (endTime <= startTime)
                throw new ArgumentException("End time must be after start time.");

            Id = Guid.NewGuid();
            SpecialistId = specialistId;
            UserId = userId;
            StartTime = startTime;
            EndTime = endTime;
            Status = ReservationStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        public void Cancel(DateTime now, TimeSpan cancellationLimit)
        {
            if (Status != ReservationStatus.Active)
                throw new InvalidOperationException("Only active reservations can be cancelled.");

            if (StartTime - now < cancellationLimit)
                throw new InvalidOperationException("Too late to cancel this reservation.");

            Status = ReservationStatus.Cancelled;
        }
    }
}
