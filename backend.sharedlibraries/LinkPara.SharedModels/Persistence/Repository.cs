#nullable enable
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using LinkPara.ContextProvider;

namespace LinkPara.SharedModels.Persistence;

public class Repository<T> : IGenericRepository<T> where T : AuditEntity
{
    private readonly DbContext _dbContext;

    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull
    {
        var entity = await _dbContext.Set<T>().FindAsync(id);

        _dbContext.Entry(entity).State = EntityState.Detached;

        return entity;
    }

    public virtual IQueryable<T> GetAll()
    {
        return _dbContext.Set<T>().AsNoTracking();
    }

    public virtual IQueryable<T> GetAll(params Expression<Func<T, object>>[] including)
    {
        var query = _dbContext.Set<T>().AsNoTracking();
        if (including != null)
            including.ToList().ForEach(include =>
            {
                if (include != null)
                    query = query.Include(include);
            });
        return query;
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        _dbContext.Set<T>().Add(entity);

        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync();
    }
    public virtual async Task UpdateRangeAsync(List<T> entity)
    {
        foreach (var item in entity)
        {
            _dbContext.Entry(item).State = EntityState.Modified;
        }
        await _dbContext.SaveChangesAsync();
    }
    public virtual async Task DeleteAsync(T entity)
    {
        entity.RecordStatus = RecordStatus.Passive;

        _dbContext.Entry(entity).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task RemoveRangeAsync(List<T> entity)
    {
        foreach (var item in entity)
        {
            item.RecordStatus = RecordStatus.Passive;
        }

        _dbContext.Entry(entity).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<List<T>> AddRangeAsync(List<T> entity)
    {
        await _dbContext.Set<T>().AddRangeAsync(entity);

        await _dbContext.SaveChangesAsync();

        return entity;
    }
}




