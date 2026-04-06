using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LinkPara.MappingExtensions.Mapping;

public static class MappingExtensions
{
    public static async Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable, int pageNumber, int pageSize,
        OrderByStatus orderBy = OrderByStatus.Asc, string sortBy = null)
    {
        if (sortBy == null)
        {
            return await PaginatedListSortingWithOneElementAsync(queryable, pageNumber, pageSize, orderBy, sortBy);
        }

        var sortByElements = sortBy.Split(",");

        return sortByElements.Length > 1
            ? await PaginatedListSortingWithMoreElementAsync(queryable, pageNumber, pageSize, orderBy, sortByElements.ToList())
            : await PaginatedListSortingWithOneElementAsync(queryable, pageNumber, pageSize, orderBy, sortBy);
    }
    private static async Task<PaginatedList<TDestination>> PaginatedListSortingWithOneElementAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize, OrderByStatus orderBy = OrderByStatus.Asc, string sortBy = null)
    {
        var count = await queryable.CountAsync();
        if (sortBy is not null)
        {
            var sortExpression = GetSortExpression<TDestination>(sortBy);
            if (sortExpression is not null)
            {
                if (orderBy == OrderByStatus.Asc)
                {
                    queryable = queryable.OrderBy(sortExpression);
                }
                else
                {
                    queryable = queryable.OrderByDescending(sortExpression);
                }
            }
        }
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize, orderBy, sortBy);
    }

    /// <summary>
    /// do pagination for queries, this method supports sorting items more than one element
    /// </summary>
    private static async Task<PaginatedList<TDestination>> PaginatedListSortingWithMoreElementAsync<TDestination>(
        this IQueryable<TDestination> queryable,
        int pageNumber, int pageSize,
        OrderByStatus orderBy = OrderByStatus.Asc,
        List<string> sortProperties = null)
    {
        var count = await queryable.CountAsync();

        if (sortProperties != null && sortProperties.Any())
        {
            IOrderedQueryable<TDestination> orderedQuery = null;

            foreach (var sortBy in sortProperties)
            {
                var sortExpression = GetSortExpression<TDestination>(sortBy);

                if (sortExpression != null)
                {
                    if (orderedQuery == null)
                    {
                        orderedQuery = orderBy == OrderByStatus.Asc
                            ? queryable.OrderBy(sortExpression)
                            : queryable.OrderByDescending(sortExpression);
                    }
                    else
                    {
                        orderedQuery = orderBy == OrderByStatus.Asc
                            ? orderedQuery.ThenBy(sortExpression)
                            : orderedQuery.ThenByDescending(sortExpression);
                    }
                }
            }

            if (orderedQuery != null)
            {
                queryable = orderedQuery;
            }
        }

        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var sortProperty = sortProperties != null
            ? string.Join(",", sortProperties)
            : null;

        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize, orderBy, sortProperty);
    }

    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(this IQueryable queryable, IConfigurationProvider configuration)
    {
        return queryable.ProjectTo<TDestination>(configuration).ToListAsync();
    }

    public static Expression<Func<TDestination, object>> GetSortExpression<TDestination>(string propertyName)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(TDestination), "entity");
            var propertyNames = propertyName.Split('.');
            Expression expression = parameter;

            foreach (var propName in propertyNames)
            {
                expression = Expression.PropertyOrField(expression, propName);
            }

            Expression conversion = Expression.Convert(expression, typeof(object));
            var sortExpression = Expression.Lambda<Func<TDestination, object>>(conversion, parameter);

            return sortExpression;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<PaginatedList<TDestination>> PaginatedListWithMappingAsync<TSource, TDestination>(
        this IQueryable<TSource> queryable,
        IMapper mapper,
        int pageNumber,
        int pageSize,
        OrderByStatus orderBy = OrderByStatus.Asc,
        string sortBy = null)
    {
        if (sortBy is null)
        {
            return await PaginatedListWithMappingSortByOneElementAsync<TSource, TDestination>(queryable, mapper, pageNumber,
                pageSize, orderBy, null);
        }

        var sortByElements = sortBy.Split(",");

        return sortByElements.Length > 1
            ? await PaginatedListWithMappingSortByMoreElementAsync<TSource, TDestination>(queryable, mapper, pageNumber,
                pageSize, orderBy, sortByElements.ToList())
            : await PaginatedListWithMappingSortByOneElementAsync<TSource, TDestination>(queryable, mapper, pageNumber,
                pageSize, orderBy, sortBy);
    }
    private static async Task<PaginatedList<TDestination>> PaginatedListWithMappingSortByOneElementAsync<TSource, TDestination>(
        this IQueryable<TSource> queryable,
        IMapper mapper,
        int pageNumber,
        int pageSize,
        OrderByStatus orderBy = OrderByStatus.Asc,
        string sortBy = null)
    {
        var count = await queryable.CountAsync();
        if (sortBy is not null)
        {
            var sortExpression = GetSortExpression<TSource>(sortBy);
            if (sortExpression is not null)
            {
                queryable = orderBy == OrderByStatus.Asc
                    ? queryable.OrderBy(sortExpression)
                    : queryable.OrderByDescending(sortExpression);
            }
        }
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(s => mapper.Map<TDestination>(s)).ToListAsync();

        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize, orderBy, sortBy);
    }

    public static async Task<PaginatedList<TDestination>> PaginatedListWithMappingSortByMoreElementAsync<TSource, TDestination>(
        this IQueryable<TSource> queryable,
        IMapper mapper,
        int pageNumber,
        int pageSize,
        OrderByStatus orderBy = OrderByStatus.Asc,
        List<string> sortBys = null)
    {
        var count = await queryable.CountAsync();

        if (sortBys != null && sortBys.Any())
        {
            IOrderedQueryable<TSource> orderedQueryable = null;

            foreach (var sortBy in sortBys)
            {
                var sortExpression = GetSortExpression<TSource>(sortBy);
                if (sortExpression is not null)
                {
                    if (orderedQueryable == null)
                    {
                        orderedQueryable = orderBy == OrderByStatus.Asc
                            ? queryable.OrderBy(sortExpression)
                            : queryable.OrderByDescending(sortExpression);
                    }
                    else
                    {
                        orderedQueryable = orderBy == OrderByStatus.Asc
                            ? orderedQueryable.ThenBy(sortExpression)
                            : orderedQueryable.ThenByDescending(sortExpression);
                    }
                }
            }

            queryable = orderedQueryable ?? queryable;
        }

        var items = await queryable.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => mapper.Map<TDestination>(s))
            .ToListAsync();

        return new PaginatedList<TDestination>(items, count, pageNumber, pageSize, orderBy, string.Join(",", sortBys));
    }
}