using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;

public class PagedResult<T>
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 1000)]
    public int PageSize { get; set; } = 50;

    [Range(0, int.MaxValue)]
    public int Total { get; set; }

    [Required]
    public List<T> Items { get; set; } = new();
}
