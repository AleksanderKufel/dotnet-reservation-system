using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationSystem.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginSerializableTransactionAsync(
            CancellationToken cancellationToken);

        Task CommitAsync(
            CancellationToken cancellationToken);

        Task RollbackAsync();

        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
