using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class PendingReviewsRequest
{
    public DateOnly? Date { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 1000)]
    public int PageSize { get; set; } = 50;
}
