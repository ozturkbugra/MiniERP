using MiniERP.Domain.Common;
using System.Linq.Expressions;

namespace MiniERP.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    IQueryable<T> GetAll();
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);

    Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default); // Doğrudan liste dönen metod

    void UpdateRange(IEnumerable<T> entities); // Toplu güncelleme imzası

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);

    Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken cancellationToken = default);


}