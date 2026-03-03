using Microsoft.EntityFrameworkCore.Storage;
using MiniERP.Application.Interfaces;

namespace MiniERP.Persistence.Services
{
    public sealed class TransactionWrapper : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public TransactionWrapper(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken) => _transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
