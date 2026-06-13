using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Tests.Application;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservationAsync_ShouldAddReservation_WhenNoConflict()
    {
        // Arrange

        var repositoryMock = new Mock<IReservationRepository>();

        var unitOfWorkMock = CreateUnitOfWorkMock();

        repositoryMock
            .Setup(r => r.GetActiveForSpecialistAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var conflictChecker = new ReservationConflictChecker();

        var service = new ReservationService(
            repositoryMock.Object,
            unitOfWorkMock.Object,
            conflictChecker);

        // Act

        await service.CreateReservationAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1));

        // Assert

        repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_ShouldThrow_WhenTimeConflictOccurs()
    {
        // Arrange

        var now = DateTime.UtcNow;

        var specialistId = Guid.NewGuid();

        var existingReservation = new Reservation(
            specialistId,
            Guid.NewGuid(),
            now.AddDays(1),
            now.AddDays(1).AddHours(1));

        var repositoryMock = new Mock<IReservationRepository>();

        var unitOfWorkMock = CreateUnitOfWorkMock();

        repositoryMock
            .Setup(r => r.GetActiveForSpecialistAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingReservation]);

        var conflictChecker = new ReservationConflictChecker();

        var service = new ReservationService(
            repositoryMock.Object,
            unitOfWorkMock.Object,
            conflictChecker);

        // Act + Assert

        await Assert.ThrowsAsync<ReservationConflictException>(
            () => service.CreateReservationAsync(
                specialistId,
                Guid.NewGuid(),
                now.AddDays(1).AddMinutes(30),
                now.AddDays(1).AddHours(1).AddMinutes(30)));

        repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<Reservation>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWorkMock()
    {
        var transactionMock =
            new Mock<IDbContextTransaction>();

        transactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(u => u.BeginSerializableTransactionAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);

        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return unitOfWorkMock;
    }
}