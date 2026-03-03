using System.Data;

namespace MiniERP.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable,CancellationToken cancellationToken = default);
}