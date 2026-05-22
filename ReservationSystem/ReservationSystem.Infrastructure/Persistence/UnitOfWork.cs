using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ReservationSystem.Application.Interfaces;
using System.Data;

namespace ReservationSystem.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ReservationDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ReservationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginSerializableTransactionAsync(
            CancellationToken cancellationToken)
        {
            _transaction = await _dbContext.Database
                .BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
        }

        public async Task CommitAsync(
            CancellationToken cancellationToken)
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
