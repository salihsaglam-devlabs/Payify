namespace LinkPara.SharedModels.Pagination;

public class SearchQueryParams
{
    public string Q { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 50;
    public OrderByStatus OrderBy { get; set; }
    public string SortBy { get; set; }
}