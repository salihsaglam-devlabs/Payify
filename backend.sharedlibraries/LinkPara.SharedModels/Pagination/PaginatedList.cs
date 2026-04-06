using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace LinkPara.SharedModels.Pagination;

public class PaginatedList<T>
{
    [JsonConstructor]
    public PaginatedList()
    {
        
    }
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public OrderByStatus OrderBy { get; set; }
    public string SortBy { get; set; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize, OrderByStatus orderBy, string sortBy)
    {
        Items = items;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        OrderBy = orderBy;
        SortBy = sortBy;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

  
}