using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Interfaces;
using MiniERP.Persistence.Context;
using System.Data;

namespace MiniERP.Persistence.Services
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Serializable,CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return new TransactionWrapper(transaction);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
