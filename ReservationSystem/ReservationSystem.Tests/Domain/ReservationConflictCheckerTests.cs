using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;
using Xunit;

namespace ReservationSystem.Tests.Domain;

public class ReservationConflictCheckerTests
{
    [Fact]
    public void HasConflict_ShouldReturnTrue_WhenTimeOverlaps()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var existing = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddHours(10),
            now.AddHours(11));

        var newReservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddHours(10).AddMinutes(30),
            now.AddHours(11).AddMinutes(30));

        var checker = new ReservationConflictChecker();

        // Act
        var result = checker.HasConflict(newReservation, new[] { existing });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasConflict_ShouldReturnFalse_WhenNoOverlap()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var existing = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddHours(10),
            now.AddHours(11));

        var newReservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddHours(11),
            now.AddHours(12));

        var checker = new ReservationConflictChecker();

        // Act
        var result = checker.HasConflict(newReservation, new[] { existing });

        // Assert
        Assert.False(result);
    }
}