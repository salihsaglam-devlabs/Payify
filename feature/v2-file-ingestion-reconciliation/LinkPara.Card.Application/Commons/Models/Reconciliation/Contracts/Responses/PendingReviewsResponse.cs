using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public class PendingReviewsResponse : ReconciliationResponseBase
{
    [Required]
    public PagedResult<ManualReview> Page { get; set; } = new();
}
