using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Time.Testing;
using Moq;
using ReservationSystem.Application.Exceptions;
using ReservationSystem.Application.Interfaces;
using ReservationSystem.Application.Services;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Domain.Enums;
using ReservationSystem.Domain.Exceptions;
using ReservationSystem.Domain.Services;

namespace ReservationSystem.Tests.Application;

public class ReservationServiceTests
{
    private static readonly DateTime Monday = new(2030, 1, 7, 0, 0, 0, DateTimeKind.Utc);

    // A valid one-hour slot within working hours.
    private static readonly DateTime SlotStart = Monday.AddHours(10);

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
            CreateSpecialistRepositoryMock(exists: true).Object,
            unitOfWorkMock.Object,
            conflictChecker,
            TimeProvider.System);

        // Act

        await service.CreateReservationAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SlotStart,
            SlotStart.AddHours(1));

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

        var specialistId = Guid.NewGuid();

        var existingReservation = new Reservation(
            specialistId,
            Guid.NewGuid(),
            Monday.AddHours(9).AddMinutes(30),
            Monday.AddHours(10).AddMinutes(30));

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
            CreateSpecialistRepositoryMock(exists: true).Object,
            unitOfWorkMock.Object,
            conflictChecker,
            TimeProvider.System);

        // Act + Assert

        await Assert.ThrowsAsync<ReservationConflictException>(
            () => service.CreateReservationAsync(
                specialistId,
                Guid.NewGuid(),
                SlotStart,
                SlotStart.AddHours(1)));

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

    [Fact]
    public async Task CreateReservationAsync_ShouldThrow_WhenSpecialistDoesNotExist()
    {
        // Arrange

        var repositoryMock = new Mock<IReservationRepository>();

        var unitOfWorkMock = CreateUnitOfWorkMock();

        var service = new ReservationService(
            repositoryMock.Object,
            CreateSpecialistRepositoryMock(exists: false).Object,
            unitOfWorkMock.Object,
            new ReservationConflictChecker(),
            TimeProvider.System);

        // Act + Assert

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateReservationAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                SlotStart,
                SlotStart.AddHours(1)));

        unitOfWorkMock.Verify(
            u => u.BeginSerializableTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 8, 0, 60)]   // before opening
    [InlineData(0, 17, 0, 60)]  // after closing
    [InlineData(0, 10, 0, 120)] // two hours long
    [InlineData(0, 10, 30, 60)] // not aligned to a slot
    [InlineData(5, 10, 0, 60)]  // Saturday
    public async Task CreateReservationAsync_ShouldThrow_WhenOutsideWorkingHoursSlot(
        int daysAfterMonday, int hour, int minute, int lengthMinutes)
    {
        // Arrange

        var unitOfWorkMock = CreateUnitOfWorkMock();

        var service = new ReservationService(
            new Mock<IReservationRepository>().Object,
            CreateSpecialistRepositoryMock(exists: true).Object,
            unitOfWorkMock.Object,
            new ReservationConflictChecker(),
            TimeProvider.System);

        var startTime = Monday.AddDays(daysAfterMonday).AddHours(hour).AddMinutes(minute);

        // Act + Assert

        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateReservationAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                startTime,
                startTime.AddMinutes(lengthMinutes)));

        unitOfWorkMock.Verify(
            u => u.BeginSerializableTransactionAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelForUserAsync_ShouldCancel_WhenMoreThanLimitBeforeStart()
    {
        // Arrange

        var now = SlotStart.AddHours(-25);

        var (service, reservation) = CreateServiceWithReservation(SlotStart, now);

        // Act

        await service.CancelForUserAsync(reservation.Id, reservation.UserId);

        // Assert

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task CancelForUserAsync_ShouldThrow_WhenLessThanLimitBeforeStart()
    {
        // Arrange

        var now = SlotStart.AddHours(-23);

        var (service, reservation) = CreateServiceWithReservation(SlotStart, now);

        // Act + Assert

        await Assert.ThrowsAsync<DomainException>(
            () => service.CancelForUserAsync(reservation.Id, reservation.UserId));

        Assert.Equal(ReservationStatus.Active, reservation.Status);
    }

    private static (ReservationService Service, Reservation Reservation) CreateServiceWithReservation(
        DateTime startTime,
        DateTime now)
    {
        var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), startTime, startTime.AddHours(1));

        var repositoryMock = new Mock<IReservationRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var service = new ReservationService(
            repositoryMock.Object,
            CreateSpecialistRepositoryMock(exists: true).Object,
            CreateUnitOfWorkMock().Object,
            new ReservationConflictChecker(),
            new FakeTimeProvider(new DateTimeOffset(now)));

        return (service, reservation);
    }

    private static Mock<ISpecialistRepository> CreateSpecialistRepositoryMock(bool exists)
    {
        var specialistRepositoryMock = new Mock<ISpecialistRepository>();

        specialistRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

        return specialistRepositoryMock;
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