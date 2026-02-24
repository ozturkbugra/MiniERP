using MiniERP.Domain.Common;

namespace MiniERP.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    IQueryable<T> GetAll();
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}