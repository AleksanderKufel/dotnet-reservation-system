using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationSystem.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginSerializableTransactionAsync(
            CancellationToken cancellationToken);

        Task SaveChangesAsync(
            CancellationToken cancellationToken);
    }
}
