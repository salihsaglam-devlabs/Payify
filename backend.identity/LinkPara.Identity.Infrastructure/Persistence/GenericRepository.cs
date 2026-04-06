#nullable enable
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LinkPara.Identity.Infrastructure.Persistence;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;

    public GenericRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull
    {
        return await _dbContext.Set<T>().FindAsync(id);
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

    public virtual IQueryable<T> GetAll()
    {
        return _dbContext.Set<T>().AsNoTracking();
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

    public virtual async Task DeleteAsync(T entity)
    {
        var recordStatus = entity.GetType().GetProperty("RecordStatus");
        
        if(recordStatus is not null)
        {
            recordStatus.SetValue(entity, RecordStatus.Passive);

            _dbContext.Entry(entity).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();
        } 
    }
}