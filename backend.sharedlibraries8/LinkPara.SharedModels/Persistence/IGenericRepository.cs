#nullable enable
using System.Linq.Expressions;

namespace LinkPara.SharedModels.Persistence;

public interface IGenericRepository<T> where T : AuditEntity
{
    Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull;
    IQueryable<T> GetAll();
    IQueryable<T> GetAll(params Expression<Func<T, object>>[] including);
    Task<T> AddAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entity);
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(List<T> entity);
    Task DeleteAsync(T entity);
    Task RemoveRangeAsync(List<T> entity);
}
