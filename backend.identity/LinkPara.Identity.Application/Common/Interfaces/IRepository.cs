#nullable enable
using System.Linq.Expressions;

namespace LinkPara.Identity.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull;
    IQueryable<T> GetAll(params Expression<Func<T, object>>[] including);
    IQueryable<T> GetAll();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}