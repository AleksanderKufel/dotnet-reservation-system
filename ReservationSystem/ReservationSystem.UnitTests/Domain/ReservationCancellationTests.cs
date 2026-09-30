using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;
using ReservationSystem.Domain.Exceptions;
using Xunit;

namespace ReservationSystem.Tests.Domain;

public class ReservationCancellationTests
{
    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled_WhenCalledBeforeLimit()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(2),
            now.AddDays(2).AddHours(1));

        var cancellationLimit = TimeSpan.FromHours(24);

        // Act
        reservation.Cancel(now, cancellationLimit);

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenCalledTooLate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddHours(5),
            now.AddHours(6));

        var cancellationLimit = TimeSpan.FromHours(24);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            reservation.Cancel(now, cancellationLimit));
    }
}