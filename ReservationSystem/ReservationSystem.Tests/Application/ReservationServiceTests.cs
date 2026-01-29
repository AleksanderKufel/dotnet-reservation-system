using Moq;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;
using Xunit;

namespace ReservationSystem.Tests.Application;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservationAsync_ShouldAddReservation_WhenNoConflict()
    {
        // Arrange
        var repositoryMock = new Mock<IReservationRepository>();
        repositoryMock
            .Setup(r => r.GetActiveForSpecialistAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reservation>());

        var conflictChecker = new ReservationConflictChecker();
        var service = new ReservationService(
            repositoryMock.Object,
            conflictChecker);

        // Act
        await service.CreateReservationAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1));

        // Assert
        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Reservation>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrow_WhenTimeConflictOccurs()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var existingReservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            now.AddDays(1),
            now.AddDays(1).AddHours(1));

        var repositoryMock = new Mock<IReservationRepository>();
        repositoryMock
            .Setup(r => r.GetActiveForSpecialistAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reservation> { existingReservation });

        var conflictChecker = new ReservationConflictChecker();
        var service = new ReservationService(
            repositoryMock.Object,
            conflictChecker);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateReservationAsync(
                existingReservation.SpecialistId,
                Guid.NewGuid(),
                now.AddDays(1).AddMinutes(30),
                now.AddDays(1).AddHours(1).AddMinutes(30)));

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Reservation>()),
            Times.Never);
    }
}